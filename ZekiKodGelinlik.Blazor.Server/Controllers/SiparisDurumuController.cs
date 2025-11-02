using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiparisDurumuController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public SiparisDurumuController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("{siparisNo}")]
        public IActionResult GetSiparisDurumu(string siparisNo)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(SiparisKarti)))
            {
                var siparis = objectSpace.FindObject<SiparisKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("icSiparisNo = ?", siparisNo));

                if (siparis == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı." });
                }

                var operasyonlar = siparis.SiparisOperasyonlar
                    .OrderBy(op => op.ModelOperasyon.islemSirasi)
                    .ThenBy(op => op.ModelOperasyon.ParalelGrup)
                    .Select(op => new
                    {
                        OperasyonAdi = op.ModelOperasyon.Operasyon.OperasyonAdi,
                        Durum = op.Durum.ToString(),
                        IslemYeri = op.ModelOperasyon.IslemYeri.ToString()
                    }).ToList();

                var sonuc = new
                {
                    SiparisNo = siparis.icSiparisNo,
                    MevcutDurum = siparis.SprDurumu?.Durumu ?? "Belirtilmemiş",
                    Operasyonlar = operasyonlar
                };

                return Ok(sonuc);
            }
        }
    }
}

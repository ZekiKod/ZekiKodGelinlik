using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SatinAlmaTalepOlusturController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public SatinAlmaTalepOlusturController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpPost]
        public IActionResult CreateSatinAlmaTalep([FromBody] SatinAlmaTalepDto talep)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(SiparisKarti)))
            {
                var siparis = objectSpace.FindObject<SiparisKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("icSiparisNo = ?", talep.SiparisNo));
                if (siparis == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı." });
                }

                foreach (var item in talep.Malzemeler)
                {
                    if (item.MalzemeTuru == "Kumaş")
                    {
                        var kumasKarti = objectSpace.FindObject<KumasKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("Kumas = ?", item.MalzemeAdi));
                        if (kumasKarti != null)
                        {
                            var satinAlma = objectSpace.CreateObject<Kumas_SatinAlma>();
                            satinAlma.SiparisKarti = siparis;
                            satinAlma.Kumas_Karti = kumasKarti;
                            satinAlma.SiparisEdilecek = item.Miktar;
                        }
                    }
                    else if (item.MalzemeTuru == "Aksesuar")
                    {
                        var malzeme = objectSpace.FindObject<MaliyetMalzemeleri>(DevExpress.Data.Filtering.CriteriaOperator.Parse("Malzeme = ?", item.MalzemeAdi));
                        if (malzeme != null)
                        {
                            var satinAlma = objectSpace.CreateObject<Malzeme_SatinAlma>();
                            satinAlma.SiparisKarti = siparis;
                            satinAlma.Malzeme = malzeme;
                            satinAlma.SiparisEdilecekMiktar = item.Miktar;
                        }
                    }
                }

                objectSpace.CommitChanges();
                return Ok(new { message = "Satın alma talepleri başarıyla oluşturuldu." });
            }
        }
    }

    public class SatinAlmaTalepDto
    {
        public string SiparisNo { get; set; }
        public List<MalzemeDto> Malzemeler { get; set; }
    }

    public class MalzemeDto
    {
        public string MalzemeTuru { get; set; }
        public string MalzemeAdi { get; set; }
        public double Miktar { get; set; }
    }
}

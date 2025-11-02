using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using System.Collections.Generic;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SatinAlmaOneriController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public SatinAlmaOneriController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("{siparisNo}")]
        public IActionResult GetSatinAlmaOnerisi(string siparisNo)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(SiparisKarti)))
            {
                var siparis = objectSpace.FindObject<SiparisKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("icSiparisNo = ?", siparisNo));

                if (siparis == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı." });
                }

                if (siparis.ModelKarti == null || siparis.OnaylanmısModel == null)
                {
                    return BadRequest(new { message = "Siparişin bir modeli veya onaylanmış maliyeti bulunmuyor." });
                }

                var oneriler = new List<object>();

                // Kumaş ihtiyaçlarını hesapla
                foreach (var kumasIhtiyac in siparis.OnaylanmısModel.MaliyetKumasCollection)
                {
                    var toplamIhtiyac = kumasIhtiyac.BirimGramaj * siparis.SiparisAdet;
                    var mevcutStok = objectSpace.GetObjects<KumasStok>(
                        DevExpress.Data.Filtering.CriteriaOperator.Parse("Kumas = ?", kumasIhtiyac.Kumas.Oid))
                        .Sum(s => s.Stok);

                    var satinAlinmasiGereken = toplamIhtiyac - mevcutStok;

                    if (satinAlinmasiGereken > 0)
                    {
                        oneriler.Add(new {
                            MalzemeTuru = "Kumaş",
                            MalzemeAdi = kumasIhtiyac.Kumas.Kumas,
                            Miktar = satinAlinmasiGereken,
                            Birim = "Gram" // Bu birim daha dinamik hale getirilebilir
                        });
                    }
                }

                // Diğer malzeme ihtiyaçlarını hesapla (Aksesuarlar vb.)
                foreach (var malzemeIhtiyac in siparis.OnaylanmısModel.MaliyetMalzemeCollection)
                {
                    var toplamIhtiyac = malzemeIhtiyac.Miktar * siparis.SiparisAdet;
                    var mevcutStok = objectSpace.GetObjects<MalzemeStok>(
                        DevExpress.Data.Filtering.CriteriaOperator.Parse("Malzeme = ?", malzemeIhtiyac.Malzeme.Oid))
                        .Sum(s => s.Stok);

                    var satinAlinmasiGereken = toplamIhtiyac - mevcutStok;

                    if (satinAlinmasiGereken > 0)
                    {
                        oneriler.Add(new {
                            MalzemeTuru = "Aksesuar",
                            MalzemeAdi = malzemeIhtiyac.Malzeme.Malzeme,
                            Miktar = satinAlinmasiGereken,
                            Birim = malzemeIhtiyac.Birim?.Birim
                        });
                    }
                }


                return Ok(oneriler);
            }
        }
    }
}

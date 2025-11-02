using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiyatTeklifiController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public FiyatTeklifiController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("ModelMaliyet/{modelNo}")]
        public IActionResult GetModelMaliyet(string modelNo)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(ModelKarti)))
            {
                var model = objectSpace.FindObject<ModelKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("ModelNo = ?", modelNo));
                if (model == null) return NotFound(new { message = "Model bulunamadı." });

                var onaylanmisMaliyet = model.Model_Maliyets.FirstOrDefault(m => m.Onayli);
                if (onaylanmisMaliyet == null) return NotFound(new { message = "Onaylanmış maliyet bulunamadı." });

                return Ok(new { MaliyetTL = onaylanmisMaliyet.ToplamTutarTL });
            }
        }

        [HttpGet("DovizKuru/{kur}")]
        public IActionResult GetDovizKuru(string kur)
        {
            // Bu örnekte, Model_Maliyet nesnesindeki kur bilgisini kullanıyoruz.
            // Gerçek bir uygulamada, bu bilgiyi merkezi bir kur servisinden almak daha doğru olabilir.
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Model_Maliyet)))
            {
                var sonMaliyet = objectSpace.GetObjects<Model_Maliyet>().OrderByDescending(m => m.Tarih).FirstOrDefault();
                if (sonMaliyet == null) return NotFound(new { message = "Kur bilgisi bulunamadı." });

                decimal kurDegeri = 0;
                switch (kur.ToUpper())
                {
                    case "USD":
                        kurDegeri = sonMaliyet.DolarKuru;
                        break;
                    case "EUR":
                        kurDegeri = sonMaliyet.EuroKuru;
                        break;
                    case "GBP":
                        kurDegeri = sonMaliyet.SterlinKuru;
                        break;
                    default:
                        return BadRequest(new { message = "Geçersiz kur birimi." });
                }

                return Ok(new { Kur = kur, Deger = kurDegeri });
            }
        }
    }
}

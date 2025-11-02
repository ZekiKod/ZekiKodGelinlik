using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelResimController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public ModelResimController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("{modelNo}")]
        public IActionResult GetModelResim(string modelNo)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(ModelKarti)))
            {
                var model = objectSpace.FindObject<ModelKarti>(DevExpress.Data.Filtering.CriteriaOperator.Parse("ModelNo = ?", modelNo));

                if (model == null)
                {
                    return NotFound(new { message = "Model bulunamadı." });
                }

                if (string.IsNullOrEmpty(model.OnResimPath))
                {
                    return NotFound(new { message = "Model için resim bulunamadı." });
                }

                // Dosya sistemindeki yolu web URL'sine dönüştür
                var imageUrl = model.OnResimPath.Replace("\\", "/");

                return Ok(new { ImageUrl = imageUrl });
            }
        }
    }
}

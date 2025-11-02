using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StokDurumuController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public StokDurumuController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("{urunAdi}")]
        public IActionResult GetStokDurumu(string urunAdi)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(KumasStok)))
            {
                var stoklar = objectSpace.GetObjects<KumasStok>(DevExpress.Data.Filtering.CriteriaOperator.Parse("Contains(Kumas.Kumas, ?)", urunAdi));

                if (stoklar == null || !stoklar.Any())
                {
                    return NotFound(new { message = "Ürün stokta bulunamadı." });
                }

                var stokDetaylari = stoklar.Select(s => new
                {
                    Depo = s.Depo,
                    StokMiktari = s.Stok,
                    Birim = s.Birim?.Birim, // Birim nesnesinin null olup olmadığını kontrol et
                    Raf = s.Raf
                }).ToList();

                return Ok(stokDetaylari);
            }
        }
    }
}

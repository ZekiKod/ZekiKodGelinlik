using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UretimTakipController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public UretimTakipController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("AktifOperasyonlar")]
        public IActionResult GetAktifOperasyonlar()
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(SiparisOperasyon)))
            {
                var operasyonlar = objectSpace.GetObjects<SiparisOperasyon>(
                    DevExpress.Data.Filtering.CriteriaOperator.Parse("Durum != ?", OperasyonDurumu.Tamamlandi)
                );

                var sonuc = operasyonlar.Select(op => new {
                    OperasyonId = op.Oid,
                    SiparisNo = op.SiparisKarti.icSiparisNo,
                    OperasyonAdi = op.ModelOperasyon.Operasyon.OperasyonAdi,
                    IslemSirasi = op.ModelOperasyon.islemSirasi,
                    ParalelGrup = op.ModelOperasyon.ParalelGrup,
                    Durum = op.Durum.ToString(),
                    IslemYeri = op.ModelOperasyon.IslemYeri.ToString(),
                    Sure = op.ModelOperasyon.Sure,
                    BaslamaTarihi = op.BaslamaTarihi
                }).ToList();

                return Ok(sonuc);
            }
        }

        [HttpPost("OperasyonDurumGuncelle")]
        public IActionResult UpdateOperasyonDurumu([FromBody] OperasyonDurumGuncelleDto dto)
        {
            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(SiparisOperasyon)))
            {
                var operasyon = objectSpace.GetObjectByKey<SiparisOperasyon>(dto.OperasyonId);
                if (operasyon == null)
                {
                    return NotFound(new { message = "Operasyon bulunamadı." });
                }

                if (Enum.TryParse<OperasyonDurumu>(dto.YeniDurum, out var yeniDurum))
                {
                    operasyon.Durum = yeniDurum;
                    if (yeniDurum == OperasyonDurumu.Basladi && operasyon.BaslamaTarihi == DateTime.MinValue)
                    {
                        operasyon.BaslamaTarihi = DateTime.Now;
                    }
                    else if (yeniDurum == OperasyonDurumu.Tamamlandi)
                    {
                        operasyon.BitisTarihi = DateTime.Now;
                    }
                    objectSpace.CommitChanges();
                    return Ok(new { message = "Operasyon durumu güncellendi." });
                }
                return BadRequest(new { message = "Geçersiz durum değeri." });
            }
        }

        [HttpPost("BildirimGonder")]
        public IActionResult SendBildirim([FromBody] BildirimDto dto)
        {
            // Gerçek bir uygulamada bu kısım e-posta, SMS veya bir bildirim servisine bağlanabilir.
            // Şimdilik, sistem loguna bir uyarı olarak yazıyoruz.
            Trace.TraceWarning($"BİLDİRİM ({dto.Kime}): {dto.Mesaj}");
            return Ok(new { message = "Bildirim gönderildi." });
        }
    }

    public class OperasyonDurumGuncelleDto
    {
        public int OperasyonId { get; set; }
        public string YeniDurum { get; set; }
    }

    public class BildirimDto
    {
        public string Kime { get; set; }
        public string Mesaj { get; set; }
    }
}

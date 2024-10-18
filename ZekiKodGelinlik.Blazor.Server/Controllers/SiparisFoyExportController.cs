using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.Persistent.Base;
using Microsoft.JSInterop;
using OfficeOpenXml;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class SiparisFoyExportController : ViewController
    {
        private IJSRuntime _jsRuntime;

        public SiparisFoyExportController()
        {
            TargetViewId = "SiparisFoy_DetailView_Prakende";
            SimpleAction sozlesmeAction = new SimpleAction(this, "Sözleşme Yazdır", PredefinedCategory.View)
            {
                Caption = "Sözleşme Yazdır",
                ImageName = "Print"
            };
            sozlesmeAction.Execute += SozlesmeAction_Execute;
            TargetObjectType = typeof(SiparisFoy);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            _jsRuntime = ((BlazorApplication)Application).ServiceProvider.GetService(typeof(IJSRuntime)) as IJSRuntime;
        }

        private async void SozlesmeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var siparisFoy = View.CurrentObject as SiparisFoy;
            if (siparisFoy == null) return;

            // Lisanslama ayarı
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Excel dosyasını oluşturma ve verileri ekleme
            byte[] fileBytes;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sayfa1");

                // Verileri yerleştir
                worksheet.Cells["B2"].Value = $"{siparisFoy.AdSoyad}";
                worksheet.Cells["B3"].Value = $"{siparisFoy.TC}";
                worksheet.Cells["B4"].Value = $"{siparisFoy.Telefon}";
                worksheet.Cells["B5"].Value = $"{siparisFoy.Adres}";

                var siparisKarti = siparisFoy.SiparisKartis.FirstOrDefault();
                if (siparisKarti?.ProvaTakips != null)
                {
                    worksheet.Cells["F2"].Value = $"{siparisKarti.ProvaTakips.FirstOrDefault()?.Tarih}";
                }

                worksheet.Cells["F4"].Value = $"{siparisFoy.MusteriTermin}";
                worksheet.Cells["F5"].Value = $"{siparisFoy.DugunTarihi}";

                worksheet.Cells["K2"].Value = $"{siparisFoy.SiparisKartis.Sum(x => x.Fiyat)}";
                worksheet.Cells["K3"].Value = $"{siparisFoy.FoyOdemePlanis.Sum(x => x.Tutar)}";
                worksheet.Cells["K4"].Value = $"{siparisFoy.KalanOdeme}";
                worksheet.Cells["B8"].Value = $"{siparisKarti?.ModelKarti.Model}";
                worksheet.Cells["B10"].Value = $"{siparisKarti?.GOGUS}";
                worksheet.Cells["B11"].Value = $"{siparisKarti?.GOGUSDUS}";
                worksheet.Cells["F8"].Value = $"{siparisKarti?.BEL}";
                worksheet.Cells["F9"].Value = $"{siparisKarti?.OMUZ}";
                worksheet.Cells["F10"].Value = $"{siparisKarti?.AYNA}";
                worksheet.Cells["F11"].Value = $"{siparisKarti?.YANBEDBOYU}";
                worksheet.Cells["J8"].Value = $"{siparisKarti?.KOLBOYU}";
                worksheet.Cells["J9"].Value = $"{siparisKarti?.PAZU}";
                worksheet.Cells["J10"].Value = $"{siparisKarti?.BILEK}";
                worksheet.Cells["J11"].Value = $"{siparisKarti?.Bilekdirskarasi}";
                worksheet.Cells["M8"].Value = $"{siparisKarti?.BOY}";
                worksheet.Cells["M9"].Value = $"{siparisKarti?.GogusAcikligi}";
                worksheet.Cells["M10"].Value = $"{siparisKarti?.SirtAcikligi}";
                worksheet.Cells["M11"].Value = $"{siparisKarti?.BASEN}";
                worksheet.Cells["A13"].Value = $"{siparisKarti?.ACIKLAMA}";
                worksheet.Cells["K12"].Value = $"{siparisFoy.Oid}";
                worksheet.Cells["F7"].Value = $"{siparisFoy.Temsilci?.AdSoyad}";

                // Excel dosyasını byte dizisine kaydet
                fileBytes = package.GetAsByteArray();
            }

            if (_jsRuntime != null)
            {
                // Dosyayı Base64 stringine dönüştür
                var base64 = Convert.ToBase64String(fileBytes);

                // JavaScript interop ile dosya indirmesini başlat
                await _jsRuntime.InvokeVoidAsync("downloadFileFromByteArray", new
                {
                    ByteArray = base64,
                    FileName = "PARAKENDE_SOZLESME.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                });
            }
        }
    }
}

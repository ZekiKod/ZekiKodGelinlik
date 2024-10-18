using OfficeOpenXml;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Services
{
    public interface IFileService
    {
        Task<byte[]> GenerateExcelFileAsync(SiparisFoy siparisFoy);
    }

    public class FileService : IFileService
    {
        public async Task<byte[]> GenerateExcelFileAsync(SiparisFoy siparisFoy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sayfa1");
                // Worksheet'i daha önceki gibi doldurun
                // ...
                return await package.GetAsByteArrayAsync();
            }
        }
    }

}

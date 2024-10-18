using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using ZekiKodGelinlik.Module.BusinessObjects;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class ExportTableSchemaController : ViewController
    {
        public ExportTableSchemaController()
        {
            SimpleAction exportSchemaAction = new SimpleAction(this, "ExportSchemaToTxt", PredefinedCategory.View)
            {
                Caption = "Export Table Schema",
                ImageName = "Action_Export_ToTxt"
            };
            exportSchemaAction.Execute += ExportSchemaAction_Execute;
        }

        private void ExportSchemaAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Dosya yolu (masaüstüne yazacak)
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SiparisFoyTableSchema.json");

            // SiparisFoy tipini alıyoruz
            Type startType = typeof(SiparisFoy);

            // Tabloları ve bağımlılıkları almak için recursive bir yöntem kullanıyoruz
            HashSet<Type> visitedTables = new HashSet<Type>(); // Ziyaret edilen tabloları takip etmek için
            List<object> jsonResults = GetTableInfoJson(startType, visitedTables);

            // Yazma işlemi
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                // JSON Formatı
                string jsonContent = JsonConvert.SerializeObject(jsonResults, Formatting.Indented);
                writer.WriteLine(jsonContent);
            }

            // Dosyayı otomatik olarak aç
            OpenFile(filePath);

            // Kullanıcıya başarı mesajı göster
            Application.ShowViewStrategy.ShowMessage("SiparisFoy tablosu ve bağımlı tabloların şeması masaüstüne başarıyla yazıldı.", InformationType.Success);
        }

        // Recursive olarak tablo ve ilişkili tabloları işleyen fonksiyon (JSON için)
        private List<object> GetTableInfoJson(Type tableType, HashSet<Type> visitedTables)
        {
            List<object> tableInfo = new List<object>();

            if (visitedTables.Contains(tableType))
            {
                return tableInfo;
            }

            visitedTables.Add(tableType);

            var properties = tableType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyNames = properties.Select(p => p.Name).ToList();

            // Tablonun JSON bilgilerini ekle
            var tableJson = new
            {
                TabloAdi = tableType.Name,
                Alanlar = propertyNames,
                BagimliTablolar = new List<object>()
            };

            // Bağımlı tabloları bul
            foreach (var prop in properties)
            {
                var associationAttribute = prop.GetCustomAttribute<AssociationAttribute>();
                var aggregatedAttribute = prop.GetCustomAttribute<AggregatedAttribute>();

                if (associationAttribute != null || aggregatedAttribute != null)
                {
                    Type relatedTableType = prop.PropertyType;

                    if (relatedTableType.IsGenericType && relatedTableType.GetGenericTypeDefinition() == typeof(XPCollection<>))
                    {
                        relatedTableType = relatedTableType.GetGenericArguments()[0];
                    }

                    // Recursive olarak bağımlı tabloyu ekle
                    tableJson.BagimliTablolar.AddRange(GetTableInfoJson(relatedTableType, visitedTables));
                }
            }

            tableInfo.Add(tableJson);
            return tableInfo;
        }

        // Dosyayı otomatik olarak açmak için bu metodu kullanıyoruz
        private void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true // Dosyayı varsayılan programla açar (Windows için)
                    });
                }
                catch (Exception ex)
                {
                    Application.ShowViewStrategy.ShowMessage($"Dosya açılamadı: {ex.Message}", InformationType.Error);
                }
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage("Dosya bulunamadı.", InformationType.Error);
            }
        }
    }
}

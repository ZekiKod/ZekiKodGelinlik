using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DevExpress.Xpo;

namespace ZekiKodGelinlik.Blazor.Server.Services
{
    public class DatabaseSchemaService
    {
        private const string SchemaCacheFile = "schema_cached.txt";

        // Şemayı yerel olarak cache etme
        public void CacheSchema(string schemaInfo)
        {
            File.WriteAllText(SchemaCacheFile, schemaInfo);
        }

        // Şemayı cache'ten alma
        public string GetCachedSchema()
        {
            if (IsSchemaCached())
            {
                return File.ReadAllText(SchemaCacheFile);
            }
            return null;
        }

        // Şemanın cache edilip edilmediğini kontrol etme
        public bool IsSchemaCached()
        {
            return File.Exists(SchemaCacheFile);
        }

        // Tüm XPO persistent sınıflarını tarayıp şema bilgisi oluşturma
        public string GenerateSchemaInformation(List<Type> types)
        {
            StringBuilder schemaBuilder = new StringBuilder();

            foreach (var type in types)
            {
                schemaBuilder.AppendLine($"Tablo: {type.Name}");
                foreach (var prop in type.GetProperties())
                {
                    schemaBuilder.AppendLine($"- Kolon: {prop.Name}, Tür: {prop.PropertyType.Name}");
                }
                schemaBuilder.AppendLine();
            }

            return schemaBuilder.ToString();
        }

        // Tüm XPO persistent sınıflarını alma
        public List<Type> GetAllXpoPersistentTypes()
        {
            var types = new List<Type>();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => typeof(XPObject).IsAssignableFrom(t)))
            {
                types.Add(type);
            }
            return types;
        }
    }
}

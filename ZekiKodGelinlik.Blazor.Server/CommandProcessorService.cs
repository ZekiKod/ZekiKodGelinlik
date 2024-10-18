using Microsoft.JSInterop;
using System.Threading.Tasks;
using OpenAI_API;
using Newtonsoft.Json;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ZekiKodGelinlik.Blazor.Server.Controllers;
using ZekiKodGelinlik.Module.BusinessObjects;
using Sirketiz.Module.BusinessObjects.Sirket_izDB;
using DevExpress.Data.Filtering;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using DevExpress.ExpressApp.Core;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;

namespace ZekiKodGelinlik.Blazor.Server
{
   

    public class CommandProcessorService
    {
        public class ServiceLocator
        {
            public static IServiceProvider ServiceProvider { get; set; }
        }
        private readonly IObjectSpaceFactory objectSpaceFactory;

        public CommandProcessorService(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [JSInvokable("ProcessCommandFromJs")]
        public static async Task ProcessCommandFromJs(string command)
        {
            // OpenAI API çağrısı ve işleme kodu
            System.Diagnostics.Debug.WriteLine("Komut alındı: " + command);

            // OpenAI API'sini çağırın ve gelen cevabı işleyin
            string apiKey = "sk-i_xeISjve8wP0IvRWnPfzVUBIM3FwLxfq42kuThiWcT3BlbkFJTXt1pq1qk5cwsA603PQTyniw2CKDRlhuKvVJc5EW4A";
            var openAiClient = new OpenAIAPI(apiKey);

            // OpenAI'ya gönderilecek metni oluşturun
            string prompt =  $@"
Sipariş adet gönderilmemişse 1 adet  olarak kabul et Aşağıdaki metni analiz et ve belirtilen JSON formatında **sadece** cevap ver. **Hiçbir açıklama, yorum veya ek metin ekleme.**

- **Tablo Yapıları ve Alanlar:**

1. **SiparisFoy**:
   - `AdSoyad`: string
   - `TC`: string
   - `Telefon`: string
   - `Adres`: string
   - `MusteriAdi`: string (eğer toptan satış ise)
   - `MusteriTermin`: datetime
   - `Toptan`: bool (kullanıcının rolüne göre)
   - `Islem`: string ('ekle' veya 'guncelle')

2. **SiparisKarti**:
   - `ModelNo`: string
   - `SiparisAdet`: int
   - `ACIKLAMA`: string
   - `BedenGrubu`: string (eğer perakende satış değilse)
   - Ölçü alanları:
     - `OMUZBOY`: string
     - `GOGUS`: string
     - `BEL`: string
     - `BASEN`: string
     - `GOGUSDUS`: string
     - `ONBEDENBOYU`: string
     - `ARKAYAKADUS`: string
     - `DISOMUZ`: string
     - `OMUZ`: string
     - `YAKADUS`: string
     - `KOLBOYU`: string
     - `BILEK`: string
     - `PAZU`: string
   - `BedenOlcu`: int

- Eğer perakende satış ise, 'BedenGrubu' alanını dahil etmeyiniz.
- Model numaraları genellikle '2210' gibi rakamlardır, ancak 'base 22 10' gibi farklı formatlarda da yazılabilir.
- Temsilci bilgisi, aktif kullanıcıdan alınacak ve 'SiparisFoy' tablosundaki 'Temsilci' alanına eklenecektir.

**Girdi Metin:**

{command}

**Çıktı Formatı (JSON):**

```json
{{
  ""SiparisFoy"": {{
    ""AdSoyad"": ""..."",
    ""TC"": ""..."",
    ""Telefon"": ""..."",
    ""Adres"": ""..."",
    ""MusteriAdi"": ""..."",
    ""MusteriTermin"": ""..."",
    ""Toptan"": true/false,
    ""Islem"": ""ekle""/""guncelle""
  }},
  ""SiparisKarti"": {{
    ""ModelNo"": ""..."",
    ""SiparisAdet"": ...,
    ""ACIKLAMA"": ""..."",
    ""BedenGrubu"": ""..."" (eğer perakende satış değilse),
    ""OMUZBOY"": ""..."",
    ""GOGUS"": ""..."",
    ""BEL"": ""..."",
    ""BASEN"": ""..."",
    ""GOGUSDUS"": ""..."",
    ""ONBEDENBOYU"": ""..."",
    ""ARKAYAKADUS"": ""..."",
    ""DISOMUZ"": ""..."",
    ""OMUZ"": ""..."",
    ""YAKADUS"": ""..."",
    ""KOLBOYU"": ""..."",
    ""BILEK"": ""..."",
    ""PAZU"": ""..."",
    ""BedenOlcu"": ...
  }}
}}";

            var chatRequest = new OpenAI_API.Chat.ChatRequest()
            {
                Model = "gpt-4",
                Messages = new List<OpenAI_API.Chat.ChatMessage>
            {
                new OpenAI_API.Chat.ChatMessage(OpenAI_API.Chat.ChatMessageRole.User, prompt)
            }
            };

            var chatResponse = await openAiClient.Chat.CreateChatCompletionAsync(chatRequest);
            var openAiCevap = chatResponse.Choices[0].Message.Content;
           
            // OpenAI'dan gelen cevabı işleyin
            System.Diagnostics.Debug.WriteLine("OpenAI Cevabı:");
            System.Diagnostics.Debug.WriteLine(openAiCevap);

            // OpenAI'dan gelen cevabı log'a yazdırın
            System.Diagnostics.Debug.WriteLine("OpenAI Cevabı:");
            System.Diagnostics.Debug.WriteLine(openAiCevap);

            // Cevabın içindeki JSON kısmını ayıkla
            var jsonString = ExtractJsonFromResponse(openAiCevap);

            if (string.IsNullOrEmpty(jsonString))
            {
                System.Diagnostics.Debug.WriteLine("JSON verisi bulunamadı.");
                return;
            }

            try
            {
                var jsonData = JsonConvert.DeserializeObject<SiparisVeri>(jsonString);

                if (jsonData == null)
                {
                    System.Diagnostics.Debug.WriteLine("JSON verisi çözümlenemedi.");
                    return;
                }
                // Servis sağlayıcısı üzerinden singleton CommandProcessorService'e erişiyoruz
                var serviceProvider = ServiceLocator.ServiceProvider;
                var commandProcessorService = serviceProvider.GetRequiredService<CommandProcessorService>();

               
                await Task.CompletedTask;
            }

      
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine("JSON parse hatası: " + ex.Message);
            }
         
        }

        private async Task VeriyiIsle(string openAiCevap)
        {

        }

        private static string ModelNumarasiniIsle(string modelNo)
        {
            if (string.IsNullOrEmpty(modelNo))
                return null;

            // 'base 22 10' gibi formatları '2210' olarak dönüştür
            modelNo = modelNo.Replace(" ", "");
            modelNo = modelNo.Replace("base", "");
            return modelNo.Trim();
        }
    
    private static string ExtractJsonFromResponse(string response)
        {
            var jsonStartIndex = response.IndexOf("{");
            if (jsonStartIndex >= 0)
            {
                var jsonString = response.Substring(jsonStartIndex).Trim();

                // JSON'un sonundaki fazladan metni kesmek için
                var lastBraceIndex = jsonString.LastIndexOf("}");
                if (lastBraceIndex >= 0)
                {
                    jsonString = jsonString.Substring(0, lastBraceIndex + 1);
                    return jsonString;
                }
            }
            return null;
        }
        // Ek olarak gerekli sınıfları tanımlayın
        public class SiparisVeri
        {
            public SiparisFoyData SiparisFoy { get; set; }
            public SiparisKartiData SiparisKarti { get; set; }
         
        }

        public class SiparisFoyData
        {
            public string AdSoyad { get; set; }
            public string TC { get; set; }
            public string Telefon { get; set; }
            public string Adres { get; set; }
            public string MusteriAdi { get; set; }
            public DateTime MusteriTermin { get; set; }
            public bool Toptan { get; set; }
            public string Islem { get; set; }
            public kisi_kartlari Temsilci { get; set; } // Temsilci alanını ekledim
        }

        public class SiparisKartiData
        {
            public string ModelNo { get; set; }
            public int SiparisAdet { get; set; }
            public string ACIKLAMA { get; set; }
            public string BedenGrubu { get; set; }
            public string OMUZBOY { get; set; }
            public string GOGUS { get; set; }
            public string BEL { get; set; }
            public string BASEN { get; set; }
            public string GOGUSDUS { get; set; }
            public string ONBEDENBOYU { get; set; }
            public string ARKAYAKADUS { get; set; }
            public string DISOMUZ { get; set; }
            public string OMUZ { get; set; }
            public string YAKADUS { get; set; }
            public string KOLBOYU { get; set; }
            public string BILEK { get; set; }
            public string PAZU { get; set; }
            public int BedenOlcu { get; set; }
        }

     
    }

}
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using OpenAI_API;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class OpenAIBarkodController : ViewController
    {
        private SimpleAction sendBarkodToOpenAI;

        public OpenAIBarkodController()
        {
            // Action tanımlaması
            sendBarkodToOpenAI = new SimpleAction(this, "SendBarkodToOpenAI", DevExpress.Persistent.Base.PredefinedCategory.Edit)
            {
                Caption = "Model Barkodunu OpenAI'ye Gönder",
                ConfirmationMessage = "Seçilen modelin barkodunu OpenAI'ye göndermek istediğinize emin misiniz?",
                ImageName = "Action_Send"
            };

            // Olayı bağlama
            sendBarkodToOpenAI.Execute += SendBarkodToOpenAI_Execute;
        }
        SiparisFoy foy; // Artık Oid yerine tüm foy nesnesini tutuyoruz.

        // Action tetiklendiğinde yapılacak işlemler
        private async void SendBarkodToOpenAI_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Seçili nesneyi al
            foy = View.CurrentObject as SiparisFoy;
            if (foy != null && !string.IsNullOrEmpty(foy.ModelBarkod))
            {
                // Barkod bilgisini OpenAI'ye gönder
                string modelBarkod = foy.ModelBarkod;
                string result = await SendToOpenAI(modelBarkod);

                // OpenAI'den dönen sonucu işle
                if (!string.IsNullOrEmpty(result))
                {
                    await VeriyiIsle(result);
                    Application.ShowViewStrategy.ShowMessage($"OpenAI yanıtı işlendi: {result}", InformationType.Success, 4000, InformationPosition.Bottom);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage("OpenAI'den yanıt alınamadı.", InformationType.Error, 4000, InformationPosition.Bottom);
                }
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage("Geçerli bir Model Barkod mevcut değil.", InformationType.Warning, 4000, InformationPosition.Bottom);
            }
        }

        // OpenAI API ile veri gönderme işlemi
        private async Task<string> SendToOpenAI(string barkodMetni)
        {
            try
            {
                string gonderilecekMetin = @$"
Lütfen aşağıdaki formata göre sipariş bilgilerini döndür:
- Satırlar {{ }} arasında gösterilmeli
- Alan adları [ ] içinde olmalı
- Değerler ( ) içinde olmalı

Örnek format:
{{ [Model]: (2200), [Adet]: (1), [Açıklama]: (""yakası açık rengi yeşil tesettürlü"") }}

Tablo Adı: SiparisKarti
Alanlar:
- Model (Örn: 2200)
- Adet (Örn: 1)
- Açıklama (Örn: ""yakası açık rengi yeşil tesettürlü"")

Tablo Adı: ModelTablosu
Alanlar:
- ModelNo
- ModelAdi

Veri Girişi:
- Sipariş: [Model], Adet: [Adet], Açıklama: [Açıklama]

Girilen Metin:
{barkodMetni}";

                var openAIClient = new OpenAIAPI("sk-i_xeISjve8wP0IvRWnPfzVUBIM3FwLxfq42kuThiWcT3BlbkFJTXt1pq1qk5cwsA603PQTyniw2CKDRlhuKvVJc5EW4A"); // API anahtarınızı buraya ekleyin
                var chatRequest = new OpenAI_API.Chat.ChatRequest()
                {
                    Model = "gpt-4",
                    Messages = new List<OpenAI_API.Chat.ChatMessage>
                    {
                        new OpenAI_API.Chat.ChatMessage
                        {
                            Role = OpenAI_API.Chat.ChatMessageRole.System,
                            Content = "Barkod ile ilgili bilgileri işleyin."
                        },
                        new OpenAI_API.Chat.ChatMessage
                        {
                            Role = OpenAI_API.Chat.ChatMessageRole.User,
                            Content =gonderilecekMetin
                        }
                    }
                };

                var chatResponse = await openAIClient.Chat.CreateChatCompletionAsync(chatRequest);
                return chatResponse.Choices[0].Message.Content; // İlk cevabı al ve döndür
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                return $"OpenAI API Hatası: {ex.Message}";
            }
        }

        // OpenAI yanıtını işleyen metod
        private async Task VeriyiIsle(string openAiCevap)
        {
            var satirlar = openAiCevap.Split(new[] { "}\n", "}" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var satir in satirlar)
            {
                var regex = new Regex(@"\((.*?)\)");
                var matches = regex.Matches(satir);

                if (matches.Count >= 3)
                {
                    string modelNo = matches[0].Groups[1].Value;
                    string adetStr = matches[1].Groups[1].Value;
                    string aciklama = matches[2].Groups[1].Value.Trim(' ', '"');
                    string action = satir.Contains("sil") ? "sil" : (satir.Contains("guncelle") ? "guncelle" : "ekle");

                    if (!int.TryParse(adetStr, out int adet))
                    {
                        adet = 1; // Eğer adet sayısal değilse, 1 olarak kabul et
                    }

                    using (var objectSpace = Application.CreateObjectSpace())
                    {
                        // Mevcut 'foy' nesnesini yeni oturumda yeniden yükleyin
                        var yeniFoy = objectSpace.GetObjectByKey<SiparisFoy>(foy.Oid);

                        // ModelKarti tablosunda modelin var olup olmadığını kontrol et
                        var modelKarti = objectSpace.FindObject<ModelKarti>(new BinaryOperator("ModelNo", modelNo));

                        if (action == "sil" && modelKarti != null)
                        {
                            // Silme işlemi
                            objectSpace.Delete(modelKarti);
                            Application.ShowViewStrategy.ShowMessage($"Model {modelNo} silindi.", InformationType.Success, 4000, InformationPosition.Bottom);
                        }
                        else if (action == "guncelle" && modelKarti != null)
                        {
                            // Güncelleme işlemi
                            var siparisKarti = objectSpace.FindObject<SiparisKarti>(new BinaryOperator("ModelKarti", modelKarti));
                            if (siparisKarti != null)
                            {
                                siparisKarti.SiparisAdet = adet;
                                siparisKarti.ACIKLAMA = aciklama;
                                siparisKarti.SiparisFoy = yeniFoy; // Mevcut SiparisFoy ile ilişkilendir
                                Application.ShowViewStrategy.ShowMessage($"Model {modelNo} güncellendi.", InformationType.Success, 4000, InformationPosition.Bottom);
                            }
                        }
                        else
                        {
                            // Yeni SiparisKarti oluşturma işlemi
                            if (modelKarti == null)
                            {
                                // Yeni ModelKarti kaydı oluştur
                                modelKarti = objectSpace.CreateObject<ModelKarti>();
                                modelKarti.ModelNo = modelNo;
                                modelKarti.ModelAdi = $"Yeni Model {modelNo}";
                            }

                            var siparisKarti = objectSpace.CreateObject<SiparisKarti>();
                            siparisKarti.ModelKarti = modelKarti;
                            siparisKarti.SiparisAdet = adet;
                            siparisKarti.Siparis = aciklama;
                            siparisKarti.SiparisFoy = yeniFoy; // Mevcut SiparisFoy ile ilişkilendir
                            Application.ShowViewStrategy.ShowMessage($"Model {modelNo} eklendi.", InformationType.Success, 4000, InformationPosition.Bottom);
                        }

                        // Değişiklikleri kaydet
                        objectSpace.CommitChanges();
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}

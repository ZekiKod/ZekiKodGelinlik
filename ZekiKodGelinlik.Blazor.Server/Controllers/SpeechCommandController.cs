using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using OpenAI_API;
using Sirketiz.Module.BusinessObjects.Sirket_izDB;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using ZekiKodGelinlik.Module.BusinessObjects;
using DevExpress.ExpressApp.Core;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class SpeechCommandController : ViewController
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _openAiApiKey;
        private static bool _temelBilgiYuklendi = false;
        private static string _temelBilgi;
        private static bool _secimYapiliyor = false;
        private XPCollection<Musteriler> _benzerMusteriler;
        private int? _selectedCustomerNumber;
        private SiparisVeri _pendingJsonData;
        private int _pendingSiparisFoyOid;
        private bool _pendingToptan;
        private readonly NavigationManager _navigationManager;
        public SpeechCommandController()
        {
            InitializeComponent();
        }

        [ActivatorUtilitiesConstructor]
        public SpeechCommandController(IJSRuntime jsRuntime, HttpClient httpClient, NavigationManager navigationManager, IObjectSpaceFactory objectSpaceFactory, IConfiguration configuration)
        {
            _objectSpaceFactory = objectSpaceFactory;
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _configuration = configuration;
            _openAiApiKey = GetOpenAiApiKey();
            _navigationManager = navigationManager;
            InitializeComponent();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ApplicationHelper.RegisterControllerInstance(this);

            if (!_temelBilgiYuklendi)
            {
                _temelBilgi = YonetimdenTemelBilgiAl();
                _temelBilgiYuklendi = true;
                System.Diagnostics.Debug.WriteLine("Temel bilgi yüklendi.");
            }
        }

        private string YonetimdenTemelBilgiAl()
        {
            return @"
{
    ""TabloAdi"": ""SiparisFoy"",
    ""Alanlar"": [
        ""Temsilci"", ""Tarih"", ""Musteriler"", ""ParaBirimi"", ""ToplamTutar"", ""MusteriTermin"", ""AdSoyad"", ""TC"", ""Telefon"", ""Adres"", ""DugunTarihi"", ""SiparisKartis"", ""FoyOdemePlanis""
    ],
    ""BagimliTablolar"": [
        {
            ""TabloAdi"": ""Musteriler"",
            ""Alanlar"": [""MusteriAdi"", ""SiparisFoys""]
        },
        {
            ""TabloAdi"": ""SiparisKarti"",
            ""Alanlar"": [""ModelNo"", ""SiparisAdet"",  ""MusteriTermin"", ""OMUZBOY"", ""GOGUS"", ""BEL"", ""BASEN"", ""GOGUSDUS"", ""ONBEDENBOYU"", ""ARKAYAKADUS"", ""DISOMUZ"", ""OMUZ"", ""YAKADUS"", ""KOLBOYU"", ""BILEK"", ""PAZU"", ""ACIKLAMA"", ""ProvaTakips""]
        },
        {
            ""TabloAdi"": ""FoyOdemePlani"",
            ""Alanlar"": [""Tutar"", ""OdemeSekli"", ""Tarih""]
        },
        {
            ""TabloAdi"": ""ProvaTakip"",
            ""Alanlar"": [""Tarih"", ""Durumu"", ""Aciklama"", ""SiparisKarti""]
        }
    ]
}";
        }

        [JSInvokable("StaticProcessCommand")]
        public static async Task StaticProcessCommand(string command)
        {
            var controllerInstance = ApplicationHelper.GetControllerInstance<SpeechCommandController>();

            if (controllerInstance != null)
            {
                await controllerInstance.ProcessCommand(command);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Controller instance bulunamadı.");
            }
        }

        public async Task ProcessCommand(string command)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessCommand başlatıldı. Komut: {command}");
            bool isSuccess = true;
            try
            {
                if (_secimYapiliyor)
                {
                    await AnnounceResult("Lütfen müşteri seçimini tamamlayın.");
                    System.Diagnostics.Debug.WriteLine("Müşteri seçimi devam ediyor, işlem durduruldu.");
                    return;
                }

                var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
                bool toptan = currentUser?.Roles.Any(r => r.Name.Contains("PRAKENDE SATIŞ SORUMLUSU")) == false;

                string prompt = GetPromptWithSalesType(command, toptan);

                System.Diagnostics.Debug.WriteLine($"OpenAI prompt hazırlanıyor: {prompt}");

                var openAiClient = new OpenAIAPI(_openAiApiKey);
                var chatRequest = new OpenAI_API.Chat.ChatRequest()
                {
                    Model = "gpt-4o",
                    Messages = new List<OpenAI_API.Chat.ChatMessage>
            {
                new OpenAI_API.Chat.ChatMessage(OpenAI_API.Chat.ChatMessageRole.User, prompt)
            }
                };

                var chatResponse = await openAiClient.Chat.CreateChatCompletionAsync(chatRequest);
                var openAiCevap = chatResponse.Choices.FirstOrDefault()?.Message.TextContent;

                System.Diagnostics.Debug.WriteLine($"OpenAI Yanıtı: {openAiCevap}");

                string jsonString = ExtractJsonFromResponse(openAiCevap);
                if (string.IsNullOrEmpty(jsonString))
                {
                    await AnnounceResult("Gelen veride bir hata var. Lütfen tekrar deneyin.");
                    System.Diagnostics.Debug.WriteLine("JSON boş veya geçersiz.");
                    return;
                }

                // "null" string değerlerini gerçek null olarak değiştirme
                jsonString = jsonString.Replace("\"null\"", "null");

                SiparisVeri jsonData;
                try
                {
                    jsonData = JsonConvert.DeserializeObject<SiparisVeri>(jsonString);
                    System.Diagnostics.Debug.WriteLine($"Parsed JSON: {JsonConvert.SerializeObject(jsonData, Formatting.Indented)}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("JSON dönüştürme hatası: " + ex.Message);
                    await AnnounceResult("Veri işlenirken bir hata oluştu. Lütfen tekrar deneyin.");
                    return;
                }

                if (jsonData == null || jsonData.SiparisFoy == null)
                {
                    await AnnounceResult("Sipariş verisi eksik veya hatalı. Lütfen verileri kontrol edin.");
                    System.Diagnostics.Debug.WriteLine("SiparisFoy verisi eksik veya null.");
                    isSuccess = false;
                    return;
                }

                using (UnitOfWork uow = new UnitOfWork())
                {
                    uow.ConnectionString = _configuration.GetConnectionString("ConnectionString");
                    System.Diagnostics.Debug.WriteLine("UnitOfWork başlatıldı.");

                    kisi_kartlari aktifKisiKartlari = currentUser?.kisi_kartlari_to;

                    if (aktifKisiKartlari != null)
                    {
                        aktifKisiKartlari = uow.GetObjectByKey<kisi_kartlari>(aktifKisiKartlari.Oid);
                        System.Diagnostics.Debug.WriteLine($"Aktif kisi_kartlari yüklendi: {aktifKisiKartlari.Oid}");
                    }

                    SiparisFoy siparisFoy = await HandleSiparisFoy(jsonData, uow, aktifKisiKartlari, toptan);
                    System.Diagnostics.Debug.WriteLine($"SiparisFoy oluşturuldu: {siparisFoy.Oid}");
                    try
                    {
                        await HandleFoyOdemePlani(jsonData, uow, siparisFoy);
                        System.Diagnostics.Debug.WriteLine("FoyOdemePlani işlendi.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("HandleFoyOdemePlani hatası: " + ex.Message);
                        await AnnounceResult("Ödeme kaydedilirken bir hata oluştu. " + ex.Message);
                    }

                    try
                    {
                        await HandleSiparisKarti(jsonData, uow, siparisFoy, toptan);
                        System.Diagnostics.Debug.WriteLine("SiparisKarti işlendi.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("HandleSiparisKarti hatası: " + ex.Message);
                        await AnnounceResult("Sipariş kaydedilirken bir hata oluştu. " + ex.Message);
                    }
                    siparisFoy.AdSoyad = jsonData.SiparisFoy.AdSoyad;
                    siparisFoy.TC = jsonData.SiparisFoy.TC;
                    siparisFoy.Telefon = jsonData.SiparisFoy.Telefon;
                    siparisFoy.Adres = jsonData.SiparisFoy.Adres;

                    if (jsonData.SiparisFoy.MusteriTermin.HasValue && jsonData.SiparisFoy.MusteriTermin.Value > DateTime.Today.AddDays(-30))
                    {
                        siparisFoy.MusteriTermin = jsonData.SiparisFoy.MusteriTermin.Value;
                    }

                    if (jsonData.SiparisFoy.DugunTarihi.HasValue)
                    {
                        siparisFoy.DugunTarihi = jsonData.SiparisFoy.DugunTarihi.Value;
                    }
                    uow.CommitChanges();
                    Musteriler musteri = null;
                    if (toptan)
                    {
                        if (jsonData.SiparisFoy.AdSoyad !="")
                        {
                            jsonData.SiparisFoy.Musteriler = jsonData.SiparisFoy.AdSoyad;
                            jsonData.SiparisFoy.AdSoyad = "";
                        }
                        musteri = await FindOrCreateMusteri(jsonData, uow, toptan, siparisFoy.Oid);
                        if (musteri != null)
                        {
                            siparisFoy.Musteriler = musteri;
                            uow.CommitChanges();
                            System.Diagnostics.Debug.WriteLine($"Musteri atandı: {musteri.Oid}");
                        }
                        else if (_secimYapiliyor)
                        {
                            // Müşteri seçimi yapılıyor, bekleyen verileri saklayın ve işlemi durdurun
                            _pendingJsonData = jsonData;
                            _pendingSiparisFoyOid = siparisFoy.Oid;
                            _pendingToptan = toptan;

                            System.Diagnostics.Debug.WriteLine("Müşteri seçimi gerektiği için işlem durduruldu.");
                            return; // İşlemi durdurun ve metottan çıkın
                        }
                    }
                    else
                    {
                        if (jsonData.SiparisFoy.Musteriler != null)
                        {
                            siparisFoy.AdSoyad = jsonData.SiparisFoy.Musteriler;
                            jsonData.SiparisFoy.Musteriler = null;

                        }
                        else
                        {
                            siparisFoy.AdSoyad = jsonData.SiparisFoy.AdSoyad;
                        }

                        System.Diagnostics.Debug.WriteLine("Perakende satış için AdSoyad atandı.");
                    }
                    _navigationManager.NavigateTo($"/SiparisFoy_DetailView_Prakende/{siparisFoy.Oid}");

                    //// Müşteri seçimi yapılmıyorsa işlemlere devam edin
                    //if (!_secimYapiliyor)
                    //{
                    //    try
                    //    {
                    //        await HandleFoyOdemePlani(jsonData, uow, siparisFoy);
                    //        System.Diagnostics.Debug.WriteLine("FoyOdemePlani işlendi.");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        System.Diagnostics.Debug.WriteLine("HandleFoyOdemePlani hatası: " + ex.Message);
                    //        await AnnounceResult("Ödeme kaydedilirken bir hata oluştu. " + ex.Message);
                    //        isSuccess = false;
                    //    }

                    //    try
                    //    {
                    //        await HandleSiparisKarti(jsonData, uow, siparisFoy, toptan);
                    //        System.Diagnostics.Debug.WriteLine("SiparisKarti işlendi.");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        System.Diagnostics.Debug.WriteLine("HandleSiparisKarti hatası: " + ex.Message);
                    //        await AnnounceResult("Sipariş kaydedilirken bir hata oluştu. " + ex.Message);
                    //        isSuccess = false;
                    //    }

                    //    if (isSuccess)
                    //    {
                    //        uow.CommitChanges();
                    //        System.Diagnostics.Debug.WriteLine("UnitOfWork commit edildi.");
                    //        await AnnounceResult("Sipariş tamamlandı.");
                    //    }
                    //    else
                    //    {
                    //        System.Diagnostics.Debug.WriteLine("İşlem sırasında hata oluştu, commit yapılmadı.");
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                isSuccess = false;
                System.Diagnostics.Debug.WriteLine("Hata oluştu: " + ex.Message);
                await AnnounceResult("İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            if (isSuccess)
            {
                System.Diagnostics.Debug.WriteLine("ProcessCommand başarıyla tamamlandı.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ProcessCommand hatalarla tamamlandı.");
            }

        }

        private async Task<SiparisFoy> HandleSiparisFoy(SiparisVeri jsonData, UnitOfWork uow, kisi_kartlari aktifKisiKartlari, bool toptan)
        {
            if (jsonData == null || jsonData.SiparisFoy == null)
            {
                throw new ArgumentNullException(nameof(jsonData), "SiparisVeri veya SiparisFoy null. Geçerli bir veri bekleniyor.");
            }

            SiparisFoy siparisFoy = new SiparisFoy(uow)
            {
                Tarih = DateTime.Now,
                Temsilci = aktifKisiKartlari,
                Toptan = toptan
            };

            System.Diagnostics.Debug.WriteLine($"Yeni SiparisFoy oluşturuldu: {siparisFoy.Oid}");
            return siparisFoy;
        }

        private async Task<Musteriler> FindOrCreateMusteri(SiparisVeri jsonData, UnitOfWork uow, bool toptan, int siparisFoy)
        {
            if (!toptan)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(jsonData.SiparisFoy.Musteriler))
            {
                return null;
            }

            var musteriler = new XPCollection<Musteriler>(uow,
                CriteriaOperator.Parse("MusteriAdi LIKE ? OR CariKodu = ?", "%" + jsonData.SiparisFoy.Musteriler + "%", jsonData.SiparisFoy.Musteriler));

            Musteriler musteri = null;

            if (musteriler.Count == 0)
            {
                musteri = new Musteriler(uow)
                {
                    MusteriAdi = jsonData.SiparisFoy.Musteriler,
                    CariKodu = GenerateCariKodu(uow)
                };
                System.Diagnostics.Debug.WriteLine($"Yeni Musteri oluşturuldu: {musteri.Oid}");
            }
            else if (musteriler.Count == 1)
            {
                musteri = musteriler[0];
                System.Diagnostics.Debug.WriteLine($"Varolan Musteri kullanıldı: {musteri.Oid}");
            }
            else
            {
                _benzerMusteriler = musteriler;
                System.Diagnostics.Debug.WriteLine($"{_benzerMusteriler.Count} benzer Musteri bulundu.");

                if (_benzerMusteriler.Count > 1)
                {
                    _secimYapiliyor = true;
                    await ShowCustomerSelectionOptions(_benzerMusteriler, siparisFoy);
                    return null;
                }
            }

            return musteri;
        }

        private async Task ShowCustomerSelectionOptions(XPCollection<Musteriler> customers, int siparisFoy)
        {
            string optionsMessage = "Birden fazla müşteri bulundu. Lütfen aşağıdaki numaralardan birini seçin:\n";

            for (int i = 0; i < customers.Count; i++)
            {
                optionsMessage += $"{i + 1}. {customers[i].MusteriAdi} - Cari Kodu: {customers[i].CariKodu}\n";
            }

            System.Diagnostics.Debug.WriteLine("Müşteri seçimi seçenekleri oluşturuldu.");

            await _jsRuntime.InvokeVoidAsync("writeToActiveCommandsBox", optionsMessage);
            await _jsRuntime.InvokeVoidAsync("activateSpeechTextBox");

            var objRef = DotNetObjectReference.Create(this);

            await _jsRuntime.InvokeVoidAsync("setPendingSelection", true, objRef, siparisFoy);
        }

        private string GenerateCariKodu(UnitOfWork uow)
        {
            string lastCariKodu = uow.ExecuteScalar("SELECT MAX(CariKodu) FROM Musteriler")?.ToString();
            System.Diagnostics.Debug.WriteLine($"Son CariKodu: {lastCariKodu}");

            int nextCariKodu = 1;

            if (!string.IsNullOrEmpty(lastCariKodu) && int.TryParse(lastCariKodu.Substring(1), out int parsedCode))
            {
                nextCariKodu = parsedCode + 1;
            }

            string newCariKodu = $"C{nextCariKodu:D5}";
            System.Diagnostics.Debug.WriteLine($"Yeni CariKodu oluşturuldu: {newCariKodu}");
            return newCariKodu;
        }

        [JSInvokable("StaticProcessCustomerSelection")]
        public static async Task StaticProcessCustomerSelection(int selectedCustomer, int siparisFoyNumber)
        {
            var controllerInstance = ApplicationHelper.GetControllerInstance<SpeechCommandController>();

            if (controllerInstance != null)
            {
                await controllerInstance.ProcessCustomerSelection(selectedCustomer, siparisFoyNumber);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Controller instance bulunamadı.");
            }
        }

        public async Task ProcessCustomerSelection(int selectedIndex, int siparisFoyNumber)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessCustomerSelection başlatıldı. Seçilen Index: {selectedIndex}, SiparisFoyNumber: {siparisFoyNumber}");
            try
            {
                if (_benzerMusteriler != null && selectedIndex > 0 && selectedIndex <= _benzerMusteriler.Count)
                {
                    Musteriler selectedMusteri = _benzerMusteriler[selectedIndex - 1];
                    _secimYapiliyor = false;
                    await AnnounceResult($"Seçilen müşteri: {selectedMusteri.MusteriAdi}");
                    System.Diagnostics.Debug.WriteLine($"Musteri seçildi: {selectedMusteri.Oid}");

                    using (var uow = new UnitOfWork())
                    {
                        uow.ConnectionString = _configuration.GetConnectionString("ConnectionString");
                        System.Diagnostics.Debug.WriteLine("Yeni UnitOfWork başlatıldı.");

                        Musteriler selectedMusteriInUow = uow.GetObjectByKey<Musteriler>(selectedMusteri.Oid);
                        SiparisFoy siparisFoy = uow.GetObjectByKey<SiparisFoy>(_pendingSiparisFoyOid);

                        if (siparisFoy != null)
                        {
                            siparisFoy.Musteriler = selectedMusteriInUow;
                            System.Diagnostics.Debug.WriteLine($"SiparisFoy.Musteriler güncellendi: {selectedMusteriInUow.Oid}");

                            //try
                            //{
                            //    await ProcessAfterCustomerSelection(_pendingJsonData, uow, siparisFoy, _pendingToptan);
                            //}
                            //catch (Exception ex)
                            //{
                            //    await AnnounceResult("İşlem sırasında bir hata oluştu: " + ex.Message);
                            //    System.Diagnostics.Debug.WriteLine("ProcessAfterCustomerSelection hatası: " + ex.Message);
                            //}

                            _pendingJsonData = null;
                            _pendingSiparisFoyOid = 0;
                            _pendingToptan = false;

                        uow.CommitChanges();
                            System.Diagnostics.Debug.WriteLine("UnitOfWork commit edildi.");
                        }
                        else
                        {
                            await AnnounceResult("Sipariş bulunamadı. İşlem tamamlanamadı.");
                            System.Diagnostics.Debug.WriteLine("SiparisFoy bulunamadı.");
                        }
                    }

                    await _jsRuntime.InvokeVoidAsync("setPendingSelection", false);
                    _selectedCustomerNumber = null;
                }
                else
                {
                    await AnnounceResult("Geçersiz seçim yapıldı. Lütfen geçerli bir numara seçin.");
                    System.Diagnostics.Debug.WriteLine("Geçersiz müşteri seçimi.");
                }
            }
            catch (Exception ex)
            {
                await AnnounceResult($"Hata oluştu: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ProcessCustomerSelection hatası: {ex.Message}");
            }
        }

        //private async Task ProcessAfterCustomerSelection(SiparisVeri jsonData, UnitOfWork uow, SiparisFoy siparisFoy, bool toptan)
        //{
        //    System.Diagnostics.Debug.WriteLine("ProcessAfterCustomerSelection başlatıldı.");
        //    try
        //    {
        //        await HandleFoyOdemePlani(jsonData, uow, siparisFoy);
        //        System.Diagnostics.Debug.WriteLine("FoyOdemePlani işlendi.");
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("HandleFoyOdemePlani hatası: " + ex.Message);
        //        await AnnounceResult("Ödeme kaydedilirken bir hata oluştu. " + ex.Message);
        //    }

        //    try
        //    {
        //        await HandleSiparisKarti(jsonData, uow, siparisFoy, toptan);
        //        System.Diagnostics.Debug.WriteLine("SiparisKarti işlendi.");
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("HandleSiparisKarti hatası: " + ex.Message);
        //        await AnnounceResult("Sipariş kaydedilirken bir hata oluştu. " + ex.Message);
        //    }

        //    System.Diagnostics.Debug.WriteLine("ProcessAfterCustomerSelection tamamlandı.");
        //}

        private string ExtractJsonFromResponse(string response)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    System.Diagnostics.Debug.WriteLine("ExtractJsonFromResponse: Response boş.");
                    return null;
                }

                int jsonStartIndex = response.IndexOf("{");
                if (jsonStartIndex < 0)
                {
                    System.Diagnostics.Debug.WriteLine("ExtractJsonFromResponse: JSON başlangıcı bulunamadı.");
                    return null;
                }

                string jsonString = response.Substring(jsonStartIndex).Trim();

                int braceCount = 0;
                int lastBraceIndex = -1;

                for (int i = 0; i < jsonString.Length; i++)
                {
                    if (jsonString[i] == '{') braceCount++;
                    if (jsonString[i] == '}') braceCount--;

                    if (braceCount == 0)
                    {
                        lastBraceIndex = i;
                        break;
                    }
                }

                if (lastBraceIndex >= 0)
                {
                    jsonString = jsonString.Substring(0, lastBraceIndex + 1);
                    System.Diagnostics.Debug.WriteLine($"ExtractJsonFromResponse: JSON başarıyla ayrıştırıldı. JSON: {jsonString}");
                    return jsonString;
                }

                System.Diagnostics.Debug.WriteLine("ExtractJsonFromResponse: JSON bitişi bulunamadı.");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ExtractJsonFromResponse hatası: " + ex.Message);
                return null;
            }
        }

        private string GetPromptWithSalesType(string command, bool toptan)
        {
            string salesType = toptan ? "Toptan" : "Perakende";

            return @$"{_temelBilgi}
Kullanıcı komutu: {command}
Satış Türü: {salesType}

Aşağıdaki tabloları ve alanları kullanarak JSON formatında bir yanıt oluştur:

- **SiparisFoy**:
  - Tarih (DateTime)
  - Musteriler (string) - Eğer 'Toptan' false ise boş bırakın
  - AdSoyad (string) - Eğer 'Toptan' true ise boş bırakın
  - TC (string)
  - Telefon (string)
  - Adres (string)
  - MusteriTermin (DateTime?)
  - Toptan (bool)
 
  - DugunTarihi (DateTime?)

- **SiparisKartiList**:
  - List of SiparisKarti objects:
    - ModelNo (string)
    - SiparisAdet (int) - Eğer belirtilmezse 1 olarak kabul et
    - Aciklama (string)
   
    - OMUZBOY (string)
    - GOGUS (string)
    - BEL (string)
    - BASEN (string)
    - GOGUSDUS (string)
    - ONBEDENBOYU (string)
    - ARKAYAKADUS (string)
    - DISOMUZ (string)
    - OMUZ (string)
    - YAKADUS (string)
    - KOLBOYU (string)
    - BILEK (string)
    - PAZU (string)
    - BedenOlcu (int)

- **FoyOdemePlaniList**:
  - List of FoyOdemePlani objects:
    - Tutar (decimal)
    - OdemeSekli (string)
    - Tarih (DateTime)

- **ProvaTakipList**:
  - List of ProvaTakip objects:
    - Tarih (DateTime)
    - Durumu (string)
    - Aciklama (string)

**Kurallar**:
- gelen veride hey mira, mira bekle, mira devam et, mira kaydet gibi veriler komut vermek için bunları görmezden gel.
- Model numarası arasındaki boşlukları kaldır, örneğin 22 0 0 aslında 2200'dür veya 21 95 aslında 2195'tir.
- Gonderdiğin verinin aynısını tekrar oluşturma buna dikkat et tek komutta çift veri oluşturma
- Eğer 'Toptan' false ise 'Musteriler' alanı boş bırakılacak, 'AdSoyad' alanı doldurulacak.
- 'Temsilci' alanına müşteri temsilcisinin adını eklemeyin. Bu alan veritabanından gelen bilgiyi içermelidir.
- Bilgiler eksikse 'null' değerleri kullanın. 'N/A' yazmayın, alanları boş bırakın.
- JSON'da tüm tabloların doldurulmasını sağlayın. Boş olan tablolar için ilgili alanlar null veya boş bırakılmalı.
- Birden fazla SiparisKarti olabilmesi için 'SiparisKartiList' bir liste olmalıdır.
";
        }

        private async Task HandleSiparisKarti(SiparisVeri jsonData, UnitOfWork uow, SiparisFoy siparisFoy, bool toptan)
        {
            System.Diagnostics.Debug.WriteLine("HandleSiparisKarti başlatıldı.");
            if (jsonData.SiparisKartiList != null && jsonData.SiparisKartiList.Count > 0)
            {
                foreach (var siparisKartiData in jsonData.SiparisKartiList)
                {
                    if (string.IsNullOrEmpty(siparisKartiData?.ModelNo))
                    {
                        System.Diagnostics.Debug.WriteLine("ModelNo boş, işlenmeyecek.");
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine($"İşleniyor: ModelNo = {siparisKartiData.ModelNo}");

                    SiparisKarti siparisKarti = uow.FindObject<SiparisKarti>(
                        CriteriaOperator.Parse("ModelKarti.ModelNo == ? AND SiparisFoy == ?", siparisKartiData.ModelNo, siparisFoy)
                    ) ?? new SiparisKarti(uow) { SiparisFoy = siparisFoy };

                    siparisKarti.SiparisAdet = siparisKartiData.SiparisAdet > 0 ? siparisKartiData.SiparisAdet : 1;
                    siparisKarti.ACIKLAMA = siparisKartiData.Aciklama;
                    siparisKarti.MusteriTermin = siparisFoy.MusteriTermin;

                    var modelKarti = uow.FindObject<ModelKarti>(new BinaryOperator("ModelNo", siparisKartiData.ModelNo))
                                        ?? new ModelKarti(uow) { ModelNo = siparisKartiData.ModelNo };
                    siparisKarti.ModelKarti = modelKarti;

                    siparisKarti.OMUZBOY = siparisKartiData.OMUZBOY;
                    siparisKarti.GOGUS = siparisKartiData.GOGUS;
                    siparisKarti.BEL = siparisKartiData.BEL;
                    siparisKarti.BASEN = siparisKartiData.BASEN;
                    siparisKarti.GOGUSDUS = siparisKartiData.GOGUSDUS;
                    siparisKarti.ONBEDENBOYU = siparisKartiData.ONBEDENBOYU;
                    siparisKarti.ARKAYAKADUS = siparisKartiData.ARKAYAKADUS;
                    siparisKarti.DISOMUZ = siparisKartiData.DISOMUZ;
                    siparisKarti.OMUZ = siparisKartiData.OMUZ;
                    siparisKarti.YAKADUS = siparisKartiData.YAKADUS;
                    siparisKarti.KOLBOYU = siparisKartiData.KOLBOYU;
                    siparisKarti.BILEK = siparisKartiData.BILEK;
                    siparisKarti.PAZU = siparisKartiData.PAZU;

                    if (toptan && !string.IsNullOrEmpty(siparisKartiData.BedenOlcu))
                    {
                        var bedenGrubu = uow.FindObject<BedenOlculeri>(new BinaryOperator("Beden", siparisKartiData.BedenOlcu))
                                        ?? new BedenOlculeri(uow) { Beden = siparisKartiData.BedenOlcu };
                        siparisKarti.BedenOlcu = bedenGrubu;
                    }

                    if (!siparisFoy.SiparisKartis.Contains(siparisKarti))
                    {
                        siparisFoy.SiparisKartis.Add(siparisKarti);
                        System.Diagnostics.Debug.WriteLine("SiparisKarti SiparisFoy'e eklendi.");
                    }

                    try
                    {
                        if (jsonData.ProvaTakipList != null)
                        {
                            await HandleProvaTakipList(jsonData, uow, siparisKarti);
                            System.Diagnostics.Debug.WriteLine("ProvaTakipList işlendi.");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("HandleProvaTakipList hatası: " + ex.Message);
                        await AnnounceResult("Prova kaydedilirken bir hata oluştu. " + ex.Message);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("HandleSiparisKarti tamamlandı.");
        }
        private async Task HandleFoyOdemePlani(SiparisVeri jsonData, UnitOfWork uow, SiparisFoy siparisFoy)
        {
            System.Diagnostics.Debug.WriteLine("HandleFoyOdemePlani başlatıldı.");
            if (jsonData.FoyOdemePlaniList != null && jsonData.FoyOdemePlaniList.Count > 0)
            {
                foreach (var odemePlaniData in jsonData.FoyOdemePlaniList)
                {
                    FoyOdemePlani odemePlani = new FoyOdemePlani(uow)
                    {
                       
                        Tutar = odemePlaniData.Tutar ?? 0,
                        Tarih = odemePlaniData.Tarih ?? DateTime.Now
                    };

                    if (!string.IsNullOrEmpty(odemePlaniData.OdemeSekli))
                    {
                        var odemeSekli = uow.FindObject<OdemeSekilleri>(new BinaryOperator("OdemeTur", odemePlaniData.OdemeSekli));
                        if (odemeSekli == null)
                        {
                            odemeSekli = new OdemeSekilleri(uow) { OdemeTur = odemePlaniData.OdemeSekli };
                            try
                            {
                                uow.CommitChanges(); // Yeni OdemeSekilleri'ni kaydediyoruz
                                System.Diagnostics.Debug.WriteLine($"Yeni OdemeSekilleri oluşturuldu: {odemeSekli.Oid}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("OdemeSekilleri kaydedilirken hata oluştu: " + ex.Message);
                                await AnnounceResult("Ödeme şekli kaydedilirken bir hata oluştu: " + ex.Message);
                                continue; // Hata oluştuysa bu ödeme planını atlıyoruz
                            }
                        }
                        odemePlani.OdemeSekli = odemeSekli;
                    }

                    siparisFoy.FoyOdemePlanis.Add(odemePlani);
                    System.Diagnostics.Debug.WriteLine("FoyOdemePlani SiparisFoy'e eklendi.");

                    try
                    {
                        uow.CommitChanges(); // Ödeme planını kaydediyoruz
                        System.Diagnostics.Debug.WriteLine("FoyOdemePlani başarıyla kaydedildi.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("FoyOdemePlani kaydedilirken hata oluştu: " + ex.Message);
                        await AnnounceResult("Ödeme planı kaydedilirken bir hata oluştu: " + ex.Message);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("FoyOdemePlani listesi boş veya null.");
            }
        }

        private async Task HandleProvaTakipList(SiparisVeri jsonData, UnitOfWork uow, SiparisKarti siparisKarti)
        {
            System.Diagnostics.Debug.WriteLine("HandleProvaTakipList başlatıldı.");
            if (jsonData.ProvaTakipList != null && jsonData.ProvaTakipList.Count > 0)
            {
                foreach (var provaTakipData in jsonData.ProvaTakipList)
                {
                    var provaTakip = new ProvaTakip(uow)
                    {
                        Tarih = provaTakipData.Tarih ?? DateTime.Now,
                        Aciklama = provaTakipData.Aciklama,
                    };

                    if (!string.IsNullOrEmpty(provaTakipData.Durumu))
                    {
                        provaTakip.Durumu = uow.FindObject<ProvaDurumu>(new BinaryOperator("Durum", provaTakipData.Durumu))
                                            ?? new ProvaDurumu(uow) { Durum = provaTakipData.Durumu };
                    }

                    siparisKarti.ProvaTakips.Add(provaTakip);
                    System.Diagnostics.Debug.WriteLine("ProvaTakip SiparisKarti'ya eklendi.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ProvaTakipList boş veya null.");
            }
        }

        private async Task AnnounceResult(string result)
        {
            System.Diagnostics.Debug.WriteLine($"AnnounceResult: {result}");
            await _jsRuntime.InvokeVoidAsync("announceResult", result);
        }

        private string GetOpenAiApiKey()
        {
            string apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenAI API anahtarı yapılandırmada bulunamadı.");
            }
            System.Diagnostics.Debug.WriteLine("OpenAI API anahtarı alındı.");
            return apiKey;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            ApplicationHelper.UnregisterControllerInstance(this);
            System.Diagnostics.Debug.WriteLine("SpeechCommandController deactivated ve unregister edildi.");
        }
    }

    public class SiparisVeri
    {
        public SiparisFoyData SiparisFoy { get; set; }
        public List<ProvaTakipData> ProvaTakipList { get; set; } = new List<ProvaTakipData>();
        public List<FoyOdemePlaniData> FoyOdemePlaniList { get; set; } = new List<FoyOdemePlaniData>();

        public List<SiparisKartiData> SiparisKartiList { get; set; } = new List<SiparisKartiData>();
    }

    public class FoyOdemePlaniData
    {
        public decimal? Tutar { get; set; }
        public string OdemeSekli { get; set; }
        public DateTime? Tarih { get; set; }
    }

    public class SiparisFoyData
    {
        public string AdSoyad { get; set; }
        public string TC { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public string Musteriler { get; set; }
        public DateTime? MusteriTermin { get; set; }
        public bool Toptan { get; set; }
        public string Islem { get; set; }
        public DateTime? DugunTarihi { get; set; }


    }

    public class SiparisKartiData
    {
        private string _modelNo;

        public string ModelNo
        {
            get { return _modelNo; }
            set { _modelNo = value?.Replace(" ", ""); }
        }

        public int SiparisAdet { get; set; } = 1;
        public string Aciklama { get; set; }

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
        public string BedenOlcu { get; set; }

        public List<ProvaTakipData> ProvaTakipList { get; set; } = new List<ProvaTakipData>();
    }

    public class ProvaTakipData
    {
        public DateTime? Tarih { get; set; }
        public string Durumu { get; set; }
        public string Aciklama { get; set; }
    }


    public static class ApplicationHelper
    {
        private static SpeechCommandController _instance;

        public static void RegisterControllerInstance(SpeechCommandController instance)
        {
            _instance = instance;
            System.Diagnostics.Debug.WriteLine("Controller instance register edildi.");
        }

        public static void UnregisterControllerInstance(SpeechCommandController instance)
        {
            if (_instance == instance)
            {
                _instance = null;
                System.Diagnostics.Debug.WriteLine("Controller instance unregister edildi.");
            }
        }

        public static T GetControllerInstance<T>() where T : class
        {
            return _instance as T;
        }
    }
}

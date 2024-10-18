using DevExpress.ExpressApp;
using DevExpress.Xpo;
using System.Linq;
using ZekiKodGelinlik.Blazor.Server.Services;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class OpenAiDatabaseSchemaController : ViewController
    {
        private readonly DatabaseSchemaService _schemaService;
        private readonly OpenAiService _openAiService;

        public OpenAiDatabaseSchemaController(DatabaseSchemaService schemaService, OpenAiService openAiService)
        {
            InitializeComponent();
            _schemaService = schemaService;
            _openAiService = openAiService;

            // Burada hangi view türü için çalışacağını belirtiyoruz
            TargetViewType = ViewType.ListView;  // ListView ve diğer View türleri için değiştirebilirsiniz.
        }

        protected override async void OnActivated()
        {
            base.OnActivated();

            // ViewController'ın tetiklendiğinden emin olmak için bir mesaj yazdıralım.
            System.Diagnostics.Debug.WriteLine("OpenAiDatabaseSchemaController tetiklendi!");

            if (!_schemaService.IsSchemaCached())
            {
                var allTypes = _schemaService.GetAllXpoPersistentTypes();
                var schemaInfo = _schemaService.GenerateSchemaInformation(allTypes);

                try
                {
                    await _openAiService.CacheSchemaAsync(schemaInfo);
                }
                catch (Exception ex)
                {
                    // Hata durumunda loglama yapabilirsiniz
                    System.Diagnostics.Debug.WriteLine($"Şema gönderim hatası: {ex.Message}");
                }
            }
        }
    }
}

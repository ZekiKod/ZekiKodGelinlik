using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using Microsoft.AspNetCore.Components;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using ZekiKodGelinlik.Blazor.Server.Components;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class SpeechRecognitionViewController : ViewController<DetailView>
    {
        public SpeechRecognitionViewController()
        {
            InitializeComponent();
            TargetObjectType = typeof(SiparisKarti); // Hedef nesnenizi belirtin
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            var stringPropertyEditor = View.Items
                 .OfType<StringPropertyEditor>()
                 .FirstOrDefault(editor => editor.PropertyName == "ACIKLAMA");

            if (stringPropertyEditor != null)
            {
                stringPropertyEditor.ControlCreated += StringPropertyEditor_ControlCreated;
            }
        }

        private void StringPropertyEditor_ControlCreated(object sender, EventArgs e)
        {
            var editor = sender as StringPropertyEditor;
            if (editor != null)
            {
                var componentModel = editor.Control as SpeechRecognitionComponentModel;
                if (componentModel != null)
                {
                    // SpeechRecognitionComponentModel'e özel ayarları burada yapabilirsiniz.
                    componentModel.InputText = "Başlangıç metni";
                }
            }
        }
    }
}

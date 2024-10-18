// PopupSizeController.cs

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor.Templates;
using DevExpress.ExpressApp.SystemModule;
using Microsoft.JSInterop;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class PopupSizeController : WindowController
    {
        protected override void OnActivated()
        {
            base.OnActivated();

            Window.TemplateChanged += Window_TemplateChanged;
        }
        private void Window_TemplateChanged(object sender, EventArgs e)
        {
            // Change the dimensions only for the View  
            // with Id set to "PermissionPolicyRole_DetailView".
            if (Window.Template is IPopupWindowTemplateSize size
                /*&& Window.View.Id == "SiparisKarti_DetailView_Prakende"*/)
            {
                size.MaxWidth = "100vw";
                size.Width = "1800px";
                size.MaxHeight = "100vh";
                size.Height = "1600px";
            }
        }
        protected override void OnDeactivated()
        {
            Window.TemplateChanged -= Window_TemplateChanged;
            base.OnDeactivated();
        }
    }
}

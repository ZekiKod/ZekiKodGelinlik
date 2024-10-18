using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Blazor.Editors;
using Microsoft.JSInterop; // IJSRuntime için gerekli
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class PopupMaxControl : ViewController
    {
        private readonly IJSRuntime jsRuntime;

        public PopupMaxControl(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
            InitializeComponent();
            PopupWindowShowAction showPopupAction = new PopupWindowShowAction(this, "ShowPopup", DevExpress.Persistent.Base.PredefinedCategory.View);
            showPopupAction.CustomizePopupWindowParams += ShowPopupAction_CustomizePopupWindowParams;
        }

        private void ShowPopupAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            // Pop-up penceresinin içeriğini ve diğer ayarlarını burada yapın.
            e.View = Application.CreateDetailView(Application.CreateObjectSpace(), typeof(SiparisKarti), true);
            e.DialogController.ViewClosed += DialogController_ViewClosed;
        }

        private void DialogController_ViewClosed(object sender, EventArgs e)
        {
            // JavaScript kodunu çağırmak için Client-side olayını ekleyin
            InvokeMaximizePopup();
        }

        private async void InvokeMaximizePopup()
        {
            await jsRuntime.InvokeVoidAsync("maximizePopup");
        }
    }
}

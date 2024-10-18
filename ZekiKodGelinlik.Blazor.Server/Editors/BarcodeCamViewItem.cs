using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using Microsoft.AspNetCore.Components;

namespace ZekiKodGelinlik.Module.Blazor.Editors
{
	public interface IModelBarcodeCamViewItem : IModelViewItem { }

    [ViewItem(typeof(IModelBarcodeCamViewItem))]
    public class BarcodeCamViewItem : ViewItem
    {
        public class DxButtonHolder : IComponentContentHolder
        {
            private readonly View currentView;
            public DxButtonHolder(View _currentView)
            {
                this.currentView = _currentView;
            }

            RenderFragment IComponentContentHolder.ComponentContent => ZekiKodGelinlik.Blazor.Server.RazorComponents.BarcodeCamInterface.Create(currentView);
        }
        public BarcodeCamViewItem(IModelViewItem model, Type objectType) : base(objectType, model.Id) { }
     
        protected override object CreateControlCore() => new DxButtonHolder(this.View);
    }

   
}

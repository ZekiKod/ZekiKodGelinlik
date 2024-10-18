using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using Microsoft.AspNetCore.Components;

namespace ZekiKodGelinlik.Module.Blazor.Editors
{
	public interface IModelToptanSiparisTakip : IModelViewItem { }

	[ViewItem(typeof(IModelToptanSiparisTakip))]
	public class ToptanSiparisTakip : ViewItem
	{
		public class DxButtonHolder : IComponentContentHolder
		{
			private readonly View currentView;
			public DxButtonHolder(View _currentView)
			{
				this.currentView = _currentView;
			}
			RenderFragment IComponentContentHolder.ComponentContent => ZekiKodGelinlik.Blazor.Server.RazorComponents.ToptanSiparisTakip.Create(currentView);
		}
		public ToptanSiparisTakip(IModelViewItem model, Type objectType) : base(objectType, model.Id) { }
		protected override object CreateControlCore() => new DxButtonHolder(this.View);
	}
}

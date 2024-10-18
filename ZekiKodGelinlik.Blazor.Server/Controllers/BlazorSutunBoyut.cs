using ZekiKodGelinlik.Module.BusinessObjects;
using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public  class ColumnResizeModeViewController : ObjectViewController<ListView,object>
    {
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            if (View.Editor is DxGridListEditor gridListEditor)
            {
                IDxGridAdapter dataGridAdapter = gridListEditor.GetGridAdapter();
                dataGridAdapter.GridModel.ColumnResizeMode = GridColumnResizeMode.ColumnsContainer;
                foreach (var columnModel in dataGridAdapter.GridDataColumnModels)
                {

                    columnModel.Width = "10%";
                    columnModel.MinWidth = 120;

                }
            }
        }
    }
}


using DevExpress.ExpressApp.Model;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    internal class CustomTextPropertyEditor
    {
        private IModelMemberViewItem model;
        private Type type;

        public CustomTextPropertyEditor(IModelMemberViewItem model, Type type)
        {
            this.model = model;
            this.type = type;
        }
    }
}
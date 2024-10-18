using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using Microsoft.JSInterop;  // JavaScript entegrasyonu için gerekli
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class AutoSaveController : ViewController
    {
        public IJSRuntime jsRuntime;
        private bool isLoading = false;

        public AutoSaveController()
        {
            InitializeComponent();
           
        }
        
        protected override void OnActivated()
        {
            base.OnActivated();

            // jsRuntime nesnesinin null olmamasını sağlıyoruz
            jsRuntime = ((BlazorApplication)Application).ServiceProvider.GetService<IJSRuntime>();
            if (jsRuntime == null)
            {
                throw new InvalidOperationException("IJSRuntime service could not be found.");
            }

            // Sayfa kapanma/yenilenme olayını dinle
            //RegisterJavaScriptEvents().ConfigureAwait(false);

            // DetailView veya ListView olup olmadığını kontrol edelim
            if (View is DetailView detailView)
            {
               
                // DetailView için olayları dinle
                ObjectSpace.ModifiedChanged += ObjectSpace_ModifiedChanged;
                detailView.Closing += View_Closing;
            }
            else if (View is ListView listView)
            {
                // ListView için olayları dinle
                listView.Editor.AllowEditChanged += Editor_AllowEditChanged;
            }
        }

        // JavaScript ile sayfa kapanma/yenilenme olaylarını dinle
        //private async System.Threading.Tasks.Task RegisterJavaScriptEvents()
        //{
        //    try
        //    {
        //        if (jsRuntime != null)
        //        {
        //            await jsRuntime.InvokeVoidAsync("registerBeforeUnloadEvent", DotNetObjectReference.Create(this));
        //            try
        //            {
        //                await jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
        //                await jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
        //                await jsRuntime.InvokeVoidAsync("JSFunctions.refreshPage");

        //            }
        //            catch (Exception ex)
        //            {
        //                Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracing.Tracer.LogError($"Error registering JS event: {ex.Message}");
        //    }
        //}

        [JSInvokable]
        public void SaveChangesBeforeUnload()
        {
            if (!isLoading)
            {
                try
                {
                    isLoading = true;
                    if (ObjectSpace.IsModified)
                    {
                        try
                        {
                            jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
                            jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
                        }
                        catch (Exception ex)
                        {
                            Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
                        }
                        ObjectSpace.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    Tracing.Tracer.LogError($"Error saving changes before unload: {ex.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

        // DetailView'de herhangi bir değişiklik olduğunda tetiklenir
        private void ObjectSpace_ModifiedChanged(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                try
                {
                    try
                    {
                       
                        jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
                        jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
                        jsRuntime.InvokeVoidAsync("JSFunctions.refreshPage");
                    }
                    catch (Exception ex)
                    {
                        Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
                    }
                    isLoading = true;
                    if (ObjectSpace.IsModified)
                    {
                        ObjectSpace.CommitChanges();
                       
                       
                    }
                }
                catch (Exception ex)
                {
                    Tracing.Tracer.LogError($"Error committing changes: {ex.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

        // ListView'deki düzenleme değişiklikleri için tetiklenen olay
        private void Editor_AllowEditChanged(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                try
                {
                    isLoading = true;
                    if (View is ListView listView && View.AllowEdit)
                    {
                        if (ObjectSpace.IsModified)
                        {
                            ObjectSpace.CommitChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracing.Tracer.LogError($"Error in editor allow edit changed: {ex.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

       
        // DetailView kapanırken tetiklenen olay
        private void View_Closing(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                try
                {
                    isLoading = true;
                    if (ObjectSpace.IsModified)
                    {
                        ObjectSpace.CommitChanges();
                        View.RefreshDataSource();
                       
                    }
                }
                catch (Exception ex)
                {
                    Tracing.Tracer.LogError($"Error saving changes on view closing: {ex.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            // Olaylardan abonelikleri kaldırıyoruz
            if (View is DetailView detailView)
            {
                ObjectSpace.ModifiedChanged -= ObjectSpace_ModifiedChanged;
                detailView.Closing -= View_Closing;
            }
            else if (View is ListView listView)
            {
                listView.Editor.AllowEditChanged -= Editor_AllowEditChanged;
            }

            // JavaScript olaylarını iptal edebiliriz
            try
            {
                if (jsRuntime != null)
                {
                    try
                    {
                         jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
                         jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
                    }
                    catch (Exception ex)
                    {
                        Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
                    }
                    jsRuntime.InvokeVoidAsync("unregisterBeforeUnloadEvent").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError($"Error unregistering JS event: {ex.Message}");
            }
        }
    }
}

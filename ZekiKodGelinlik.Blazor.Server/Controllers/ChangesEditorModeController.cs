using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System.ComponentModel;
using ListView = DevExpress.ExpressApp.ListView;

namespace Solution2.Module.Win.BusinessObjects
{
    public class ChangesEditorModeController : ViewController<ListView>
    {
        private IContainer components = null;
        private SimpleAction simpleAction1;
        //private SimpleAction simpleAction2;

        public ChangesEditorModeController()
        {
            InitializeComponent();
        }

        private void SimpleAction1_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Save the current View and extract it from the Frame.
            ListView savedView = (ListView)Frame.View;
            
          
                if (Frame.SetView(null, true, null, false))
                {
                    // Make required changes to the related Application Model nodes here.
                    MasterDetailMode defaultMasterDetailMode = MasterDetailMode.ListViewOnly;
                    savedView.Model.MasterDetailMode = savedView.Model.MasterDetailMode == defaultMasterDetailMode ? MasterDetailMode.ListViewAndDetailView : defaultMasterDetailMode;
                // Update the saved View according to the latest model changes and assign it to the current Frame.
                if (savedView != null && savedView.Model != null)
                {

                    savedView.LoadModel(false);
                    Frame.SetView(savedView);
                }
                // Optionally update the current object of the inner DetailView (this code is no longer required in v16.2.7 or v17.1.4).
                if (savedView.EditView != null)
                    {
                        savedView.EditView.CurrentObject = savedView.CurrentObject;
                    }
                }
            
        }

        //private void SimpleAction2_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    var savedView = View;
        //    if (Frame.SetView(null, true, null, false))
        //    {
        //        savedView.Model.EditorType = savedView.Model.EditorType == typeof(TreeListEditor) ? typeof(GridListEditor) : typeof(TreeListEditor);
        //        savedView.LoadModel(false);
        //        Frame.SetView(savedView);
        //    }
        //}

        private void InitializeComponent()
        {
           
                this.components = new Container();
                this.simpleAction1 = new SimpleAction(this.components);
                //this.simpleAction2 = new SimpleAction(this.components);
                // 
                // simpleAction1
                // 
                this.simpleAction1.Caption = "Detay  Göster / Gizle";
                this.simpleAction1.Category = "View";
                this.simpleAction1.ConfirmationMessage = null;
                this.simpleAction1.Id = "simpleAction1";
                this.simpleAction1.ToolTip = null;
                this.simpleAction1.ImageName = "BO_Product";
                this.simpleAction1.Execute += SimpleAction1_Execute;
            
          
            // 
            // simpleAction2
            // 
            //this.simpleAction2.Caption = "SwitchEditor";
            //this.simpleAction2.Category = "View";
            //this.simpleAction2.ConfirmationMessage = null;
            //this.simpleAction2.Id = "simpleAction2";
            //this.simpleAction2.ToolTip = null;
            //this.simpleAction2.ImageName = "BO_Product";
            //this.simpleAction2.Execute += SimpleAction2_Execute;
            // 
            // ViewController1
            // 
            this.Actions.Add(this.simpleAction1);
            //this.Actions.Add(this.simpleAction2);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

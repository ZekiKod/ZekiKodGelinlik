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
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public partial class MailMergeViewController : ViewController
    {
        public MailMergeViewController()
        {
            InitializeComponent();
            TargetObjectType = typeof(Kiralama_Satis);
            SimpleAction mailMergeAction = new SimpleAction(this, "MailMerge", DevExpress.Persistent.Base.PredefinedCategory.Edit)
            {
                Caption = "Mail Merge",
                ImageName = "Action_MailMerge"
            };
            mailMergeAction.Execute += MailMergeAction_Execute;
        }

        private void MailMergeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var selectedObjects = View.SelectedObjects.Cast<Kiralama_Satis>().ToList();
            if (selectedObjects.Count > 0)
            {
                foreach (var obj in selectedObjects)
                {
                    ProcessDocument(obj);
                }
            }
        }

        private void ProcessDocument(Kiralama_Satis obj)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Template.docx");
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "MusteriSozlesmeleri");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string outputFileName = Path.Combine(directoryPath, $"{obj.TCNo}_{obj.MusteriAdi}_sozlesme.docx");

            File.Copy(templatePath, outputFileName, true);

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(outputFileName, true))
            {
                var mainPart = wordDocument.MainDocumentPart;
                string docText = null;

                using (StreamReader reader = new StreamReader(mainPart.GetStream()))
                {
                    docText = reader.ReadToEnd();
                }

                docText = docText.Replace("<<musteriadi>>", obj.MusteriAdi);
                docText = docText.Replace("<<tcno>>", obj.TCNo);
                docText = docText.Replace("<<telefon>>", obj.Telefonu);

                using (StreamWriter writer = new StreamWriter(mainPart.GetStream(FileMode.Create)))
                {
                    writer.Write(docText);
                }

                mainPart.Document.Save();
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = outputFileName,
                UseShellExecute = true
            });
        }
    }
}

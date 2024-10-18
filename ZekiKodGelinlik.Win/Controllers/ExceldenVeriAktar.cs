using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using NPOI.XSSF.UserModel;
using System.Data;
using System.IO;

using DevExpress.XtraGrid.Views.Base;
using DevExpress.ExpressApp.Win.Editors;

namespace ZekiKodGelinlik.Win.Controllers
{
    public partial class ExceldenVeriAktar : ViewController<ListView>
    {
        private RepositoryItemLookUpEdit BagliKolonEdit;

        public ExceldenVeriAktar()
        {
            
            var saveAction = new SimpleAction(this, "Excelveri", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            saveAction.Execute += SaveAction_Execute;
            BagliKolonEdit = new RepositoryItemLookUpEdit();
        }

        XtraForm form;
        GridControl gridControl;
        GridView gridView;
        DataTable dataTablel;

        private void SaveAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            form = new XtraForm();
            gridControl = new GridControl();
            gridView = new GridView();
            var LvrColumn = new GridColumn();
            var ExcelColumn = new GridColumn();
            var BagliKolonColumn = new GridColumn();

            var dataTable = new DataTable();
            dataTable.Columns.Add("ListViewKolonAd");
            dataTable.Columns.Add("ExcelKolonAd");
            dataTable.Columns.Add("BagliKolonAd");

            LvrColumn.FieldName = "ListViewKolonAd";
            LvrColumn.Caption = "ListViewKolonAd";
            LvrColumn.OptionsColumn.AllowEdit = true;
            LvrColumn.Visible = true;
            LvrColumn.VisibleIndex = 0;

            ExcelColumn.FieldName = "ExcelKolonAd";
            ExcelColumn.Caption = "ExcelKolonAd";
            ExcelColumn.OptionsColumn.AllowEdit = true;
            ExcelColumn.Visible = true;
            ExcelColumn.VisibleIndex = 1;

            BagliKolonColumn.FieldName = "BagliKolonAd";
            BagliKolonColumn.Caption = "BagliKolonAd";
            BagliKolonColumn.OptionsColumn.AllowEdit = true;
            BagliKolonColumn.Visible = true;
            BagliKolonColumn.VisibleIndex = 2;

            gridView.Columns.AddRange(new[] { LvrColumn, ExcelColumn, BagliKolonColumn });
            gridControl.DataSource = dataTable;

            var LvEdit = new RepositoryItemLookUpEdit();
            var ExcelEdit = new RepositoryItemLookUpEdit();

            LvrColumn.ColumnEdit = LvEdit;
            ExcelColumn.ColumnEdit = ExcelEdit;
            BagliKolonColumn.ColumnEdit = BagliKolonEdit;

            List<string> listColumns = new List<string>();
            foreach (var column in View.Model.Columns)
            {
                listColumns.Add(column.FieldName);
            }


            LvEdit.DataSource = listColumns;
            LvEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            LvEdit.EditValueChanged += LvEdit_EditValueChanged; // Bu satırı ekleyin

            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Excel files|*.xlsx;*.xls";
                if (fileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                var fileName = fileDialog.FileName;
                dataTablel = ReadExcelFile(fileName);

                if (!string.IsNullOrEmpty(fileDialog.FileName))
                {
                    using (var fileStream = new FileStream(fileDialog.FileName, FileMode.Open))
                    {
                        var workbook = new XSSFWorkbook(fileStream);
                        var sheet = workbook.GetSheetAt(0);

                        // Read Excel column names and load them into ExcelEdit control
                        var ExcelKolonlar = new List<string>();
                        var headerRow = sheet.GetRow(0);
                        for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
                        {
                            ExcelKolonlar.Add(headerRow.GetCell(i).StringCellValue);
                        }
                        ExcelEdit.DataSource = ExcelKolonlar;
                        ExcelEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    }
                }
            }
            gridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
            gridControl.MainView = gridView;
            gridControl.Dock = DockStyle.Fill;
            form.Controls.Add(gridControl);
            Button button = new Button()
            {
                Text = "Kaydet",
                Dock = DockStyle.Bottom
            };
            button.Click += SaveButton_Click;
            form.Controls.Add(button);
            gridView.CustomRowCellEdit += GridView_CustomRowCellEdit;
            // Show the form
            form.Show();
        }
        private void GridView_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName == "BagliKolonAd")
            {
                GridView view = sender as GridView;
                object row = view.GetRow(e.RowHandle);

                if (row != null)
                {
                    RepositoryItemLookUpEdit repLookupEdit = new RepositoryItemLookUpEdit();

                    var LvEditValue = (row as DataRowView)["ListViewKolonAd"];
                    if (LvEditValue != null)
                    {
                        var selectedModelColumn = View.Model.Columns.FirstOrDefault(c => c.FieldName == LvEditValue.ToString());
                        if (selectedModelColumn != null)
                        {
                            var dataSourceProperty = View.ObjectTypeInfo.FindMember(selectedModelColumn.PropertyName);
                            if (dataSourceProperty != null)
                            {
                                var dataSourceProperties = dataSourceProperty.MemberType.GetProperties().Select(p => p.Name).ToList();
                                repLookupEdit.DataSource = dataSourceProperties;
                            }
                        }
                    }

                    // LookupEdit için gerekli ayarlamaları yapın
                    repLookupEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

                    e.RepositoryItem = repLookupEdit;
                }
            }
        }
        private RepositoryItemLookUpEdit selectedLookupEdit;

        private void LvEdit_EditValueChanged(object sender, EventArgs e)
        {


            var lookupEdit = sender as LookUpEdit;
            if (lookupEdit == null) return;

            var winGridListEditor = (GridListEditor)View.Editor;
            if (winGridListEditor == null) return;

            var selectedModelColumn = View.Model.Columns.FirstOrDefault(c => c.FieldName == lookupEdit.EditValue?.ToString());
            if (selectedModelColumn == null) return;

            var dataSourceProperty = View.ObjectTypeInfo.FindMember(selectedModelColumn.PropertyName);
            if (dataSourceProperty == null) return;

            if (selectedModelColumn.PropertyEditorType.Name == "LookupPropertyEditor")
            {
                var dataSourceProperties = dataSourceProperty.MemberType.GetProperties().Select(p => p.Name).ToList();
                BagliKolonEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

                if (selectedLookupEdit == BagliKolonEdit)
                {
                    // Sadece seçili olan LookUpEdit için DataSource ayarlanır
                    if (selectedModelColumn.Index < View.Model.Columns.Count)
                    {
                        BagliKolonEdit.DataSource = dataSourceProperties;
                    }
                }

                var genericArguments = View.ObjectTypeInfo.FindMember(selectedModelColumn.PropertyName).MemberType.GetGenericArguments();
            }

            selectedLookupEdit = BagliKolonEdit;
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            var objectSpace = View.ObjectSpace;

            var columnMappings = new Dictionary<string, Tuple<string, string>>();

            for (int i = 0; i < gridView.DataRowCount; i++)
            {
                DataRow row = gridView.GetDataRow(i);
                string listViewColumn = row[0].ToString();
                string excelColumn = row[1].ToString();
                string relatedColumn = row[2].ToString();
                columnMappings.Add(excelColumn, Tuple.Create(listViewColumn, relatedColumn));
            }

            foreach (DataRow row in dataTablel.Rows)
            {
                var targetObject = objectSpace.CreateObject(View.ObjectTypeInfo.Type);
                foreach (var columnMapping in columnMappings)
                {
                    var excelColumnName = columnMapping.Key;
                    var targetPropertyName = columnMapping.Value.Item1;
                    var relatedPropertyName = columnMapping.Value.Item2;
                    // Noktadan sonraki kısmı çıkarın
                    string actualPropertyName = targetPropertyName.Contains(".") ? targetPropertyName.Split('.')[0] : targetPropertyName;

                    var propertyInfo = View.ObjectTypeInfo.Type.GetProperty(actualPropertyName);
                    if (propertyInfo == null) continue;

                    var modelColumn = View.Model.Columns.FirstOrDefault(c => c.PropertyName == actualPropertyName);
                    if (modelColumn == null) continue;

                    string propertyEditorTypeName = modelColumn.PropertyEditorType.Name;

                    var excelColumnIndex = dataTablel.Columns.IndexOf(excelColumnName);

                    if (propertyInfo != null && excelColumnIndex >= 0 && row[excelColumnIndex] != DBNull.Value)
                    {
                        if (propertyInfo.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(row[excelColumnIndex].ToString(), out DateTime dateTimeValue))
                            {
                                propertyInfo.SetValue(targetObject, dateTimeValue);
                            }
                            else
                            {
                                throw new Exception($"Geçersiz tarih formatı: {row[excelColumnIndex].ToString()}");
                            }
                        }
                        else if (propertyEditorTypeName == "LookupPropertyEditor" && !string.IsNullOrEmpty(relatedPropertyName))
                        {
                            var targetColumn = View.Model.Columns.FirstOrDefault(c => c.PropertyName == actualPropertyName);
                            if (targetColumn == null) continue;

                            var propertyEditorType = targetColumn.PropertyEditorType;

                            if (propertyEditorType == typeof(LookupPropertyEditor) && !string.IsNullOrEmpty(relatedPropertyName))
                            {
                                var relatedPropertyInfo = propertyInfo.PropertyType;
                                var relatedObjectType = relatedPropertyInfo;
                                var relatedObjectList = objectSpace.GetObjects(relatedObjectType);

                                // BağliKolonEdit değeri ile karşılaştırma yaparak ilgili nesneyi arayın
                                var relatedObject = relatedObjectList.Cast<object>().FirstOrDefault(o => o.GetType().GetProperty(relatedPropertyName).GetValue(o).ToString() == row[excelColumnIndex].ToString());

                                if (relatedObject == null)
                                {
                                    // BağliKolonEdit değeri ile eşleşen nesne yoksa, yeni bir nesne oluşturun ve ayarlayın
                                    relatedObject = objectSpace.CreateObject(relatedObjectType);
                                    relatedObject.GetType().GetProperty(relatedPropertyName).SetValue(relatedObject, row[excelColumnIndex]);
                                    objectSpace.CommitChanges();
                                }

                                propertyInfo.SetValue(targetObject, relatedObject);
                            }



                        }

                        else
                        {
                            propertyInfo.SetValue(targetObject, row[excelColumnIndex]);
                        }
                    }
                }
               objectSpace.CommitChanges();
            }

            form.DialogResult = DialogResult.OK;
            form.Close();
            // Refresh ListView data
            View.ObjectSpace.Refresh();
            View.Refresh();
        }


        private DataTable ReadExcelFile(string fileName)
        {
            var dataTable = new DataTable();
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var workbook = new XSSFWorkbook(fileStream);
                var sheet = workbook.GetSheetAt(0);
                var headerRow = sheet.GetRow(0);

                // Excel dosyasının ilk satırının boş olup olmadığını kontrol edin
                if (headerRow == null)
                {
                    throw new Exception("Excel dosyanızın ilk satırı boş. Lütfen bir başlık satırı ekleyin.");
                }

                for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
                {
                    // Hücrenin boş olup olmadığını kontrol edin
                    var cell = headerRow.GetCell(i);
                    if (cell != null)
                    {
                        dataTable.Columns.Add(cell.StringCellValue);
                    }
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var dataRow = dataTable.NewRow();
                    for (int j = row.FirstCellNum; j < row.LastCellNum; j++)
                    {
                        // Hücrenin boş olup olmadığını kontrol edin
                        var cell = row.GetCell(j);
                        if (cell != null)
                        {
                            dataRow[j] = cell.ToString();
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }


    }
}

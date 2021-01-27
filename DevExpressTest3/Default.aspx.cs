using DevExpress.Data.Filtering;
using DevExpress.DataAccess.Sql;
using DevExpress.Export;
using DevExpress.Web;
using DevExpress.Web.Bootstrap;
using DevExpress.XtraPrinting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;

namespace DevExpressTest3
{
    
    public partial class QueryViewer : System.Web.UI.Page
    {
        /// <summary>
        /// metodo eseguito al caricamento della pagina, e ad ogni postback di ogni suo controllo.
        /// Questo significa che spesso è necessario controllare l'output di un certo controllo qui dentro
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            SetTableParamMatesVisible(false);
            if (Session["SQL"] != null)
            {
                if (DbManager.GetParameters((string)Session["NAME"]) != null)
                {
                    SetParameters();
                    SetTableParamMatesVisible(true);
                }
                GridViewSqlDataSource.SelectCommand = (string)Session["SQL"];
                NewTeamGridView.DataBind();
                SetGridViewControlMatesVisible(true);
            }
            else SetGridViewControlMatesVisible(false);
        }
        /// <summary>
        /// Imposta la tabella dei parametri ed i controlli nello stesso ambito(label con query risultante, bottone per validare i parametri), visibili o invisibili
        /// </summary>
        /// <param name="visible">indica se i controlli in questione debbano essere visibili o meno. quando true sono visibili, false invisibili</param>
        private void SetTableParamMatesVisible(bool visible)
        {
            NewTeamControlManager.SetControlsVisible(visible, ParamTable, ResultQueryLabel, ValidateParametersButton);
        }
        /// <summary>
        /// Metodo eseguito Quando si clicca sul bottone per essere reindirizzati al querybuilder.
        /// Se una query è selezionata, il parametro NAME della sessione viene impostato con il nome della query selezionata, altrimenti null
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void QueryBuilderButton_Click(object sender, EventArgs e)
        {
            if (RadioButtonQueryList.SelectedItem != null)
            {
                Session["NAME"] = RadioButtonQueryList.SelectedItem.Text;
            }
            else Session["NAME"] = null;
            Response.Redirect("~/Query.aspx");
        }
        /// <summary>
        /// Metodo eseguito Quando si clicca su una delle due immagini bottone raffiguranti i loghi di pdf e xlsx
        /// A seconda del chiamante usa L'exporter per esportare la gridview in xlsx o pdf
        /// </summary>
        /// <param name="sender">l'immagine-bottone cliccata</param>
        /// <param name="e"></param>
        protected void FormatLogo_Click(object sender, EventArgs e)
        {
            NewTeamGridViewExporter.FileName = FileNameTextBox.Text;
            if ((sender as ImageButton).DescriptionUrl == "xlsx")NewTeamGridViewExporter.WriteXlsxToResponse(new XlsxExportOptionsEx { ExportType = ExportType.WYSIWYG });
            else NewTeamGridViewExporter.WritePdfToResponse();
        }
        /// <summary>
        /// Imposta la gridview ed i controlli nello stesso ambito(le 2 immagini bottone,label che indica il nome del file da esportare, textbox dove inserire il nome del file da esportare), visibili o invisibili
        /// </summary>
        /// <param name="visible">indica se i controlli in questione debbano essere visibili o meno. quando true sono visibili, false invisibili</param>
        private void SetGridViewControlMatesVisible(bool visible)
        {
            NewTeamControlManager.SetControlsVisible(visible,NewTeamGridView, XlsxLogoImageButton, PdfLogoImageButton, FileNameTextBox, FileNameLabel);
            NewTeamControlManager.AssignTextBoxFileName(Visible, RadioButtonQueryList, FileNameTextBox);
        }
        /// <summary>
        /// Ricarica i valori che la gidview deve assumere,basandosi sul controllo di tipo sqldatasource a cui è associato ogni volta un diverso selectCommand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void NewTeamGridView_DataBinding(object sender, EventArgs e)
        {
            NewTeamGridView.DataSource = GridViewSqlDataSource;
            NewTeamGridView.Columns.Clear();
            AddColumns(sender as ASPxGridView);
        }
        #region Column creation
        /// <summary>
        /// Aggiunge colonne alla gridview, operando con proiezione e selezione
        /// </summary>
        /// <param name="grid">la ASPxGridView su cui aggiungere le colonne</param>
        private void AddColumns(ASPxGridView grid)
        {
            if (grid == null)
            {
                NewTeamGridView.Visible = false;
                return;
            }
            System.Web.UI.WebControls.SqlDataSource sds = (System.Web.UI.WebControls.SqlDataSource)grid.DataSource;
            DataView dw= (DataView)sds.Select(DataSourceSelectArguments.Empty);
            try
            {
                foreach (DataColumn c in dw.Table.Columns)
                {
                    AddColumn(grid, c.ColumnName, c.DataType);
                }
            }
            catch
            {
                NewTeamGridView.Visible = false;
            }
        }
        static readonly Dictionary<Type, Type> columnMap = new Dictionary<Type, Type>()
        {
            { typeof(int), typeof(GridViewDataSpinEditColumn) },
            { typeof(DateTime), typeof(GridViewDataDateColumn) },
            { typeof(byte[]), typeof(GridViewDataBinaryImageColumn) }
            // more datatype/columntype mappings here...
        };
        /// <summary>
        /// Aggiunge una colonna alla tabella, specificando nome, tipo
        /// </summary>
        /// <param name="grid">la AspxGridview su cui operare</param>
        /// <param name="fieldName">il nome della colonna</param>
        /// <param name="fieldType">il tipo del campo</param>
        private static void AddColumn(ASPxGridView grid, string fieldName, Type fieldType)
        {
            Type colType = columnMap.ContainsKey(fieldType) ? columnMap[fieldType] : typeof(GridViewDataTextColumn);
            GridViewDataColumn c = Activator.CreateInstance(colType) as GridViewDataColumn;
            c.FieldName = fieldName;
            grid.Columns.Add(c);
        }

        #endregion
        /// <summary>
        /// Metodo che viene eseguito ogni volta che dalla lista delle query vienne selezionata una query non precedemente selzionata.
        /// Metodo che viene eseguito subito dopo il metodo page_init, in quanto la radiobutton list ha il flag autopostback=true.
        /// Ogni volta che il metodo è chiamato la gridview è ricaricata con i valori corrispondenti alla query selezionata, e la tabella dei parametri ripulita e ricaricata anch'essa con i valori dei parametri associati alla query.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RadioButtonQueryList_ValueChanged(object sender, EventArgs e)
        {
            Session["SQL"] = DbManager.GetSqlFromQueryName(RadioButtonQueryList.SelectedItem.Text);
            GridViewSqlDataSource.SelectCommand = (string)Session["SQL"];
            Session["NAME"] = RadioButtonQueryList.SelectedItem.Text;
            SetGridViewControlMatesVisible(true);
            NewTeamGridView.DataBind();
            ParamTable.Controls.Clear();
            if (DbManager.GetParameters(RadioButtonQueryList.SelectedItem.Text) != null)
            {
                SetTableParamMatesVisible(true);
                SetParameters();
            }
            else SetTableParamMatesVisible(false);
        }
        /// <summary>
        /// Imposta la tabella dei parametri, basandosi sui valori NAME E SQL della sessione
        /// </summary>
        private void SetParameters()
        {
            try
            {
                System.Xml.Linq.XElement paramsXelement = DbManager.GetParameters(Session["NAME"].ToString());
                ResultQueryLabel.Text = Session["SQL"].ToString();
                NewTeamControlManager.BuildParamTable(paramsXelement,ParamTable);
                SetTableParamMatesVisible(true);
            }
            catch { SetTableParamMatesVisible(false); }
        }
        /// <summary>
        /// Metodo che viene eseguito ad ogni click del bottone per validare i parametri.
        /// Questo metodo modifica la stringa di filtro della query, ottenendo altri operatori di confronto dalla tabella dei parametri.
        /// Basandosi quindi sulla stringa di filtro modificata, ricarica la gridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ValidateParametersButton_Click(object sender, EventArgs e)
        {
            OperandValue[] parameters;
            string filter = SelectQueryManager.DeserializeFromXml((string)Session["NAME"]).FilterString;
            CriteriaOperator op = CriteriaOperator.Parse(filter, out parameters);
            op=NewTeamControlManager.GetCriteriaOperatorFromPreviousValueAndParamTable(op, ParamTable);
            BindGridViewFromCriteriaOperator(op);
        }
        /// <summary>
        /// Questo metodo modifica il selectCommand della datasource associata alla gridview, tramite la nuova stringa di filtro in input.
        /// Quindi ricarica la gridview con la nuova stringa di filtro
        /// </summary>
        /// <param name="filterCriteriaOperator">Rappresenta la stringa di filtro, non come stringa ma come oggetto CriteriaOperator</param>
        private void BindGridViewFromCriteriaOperator(CriteriaOperator filterCriteriaOperator)
        {
            string filter = filterCriteriaOperator.LegacyToString();
            SelectQuery ParamSelectQuery = SelectQueryManager.DeserializeFromXml((string)Session["NAME"]);
            ParamSelectQuery.FilterString = filterCriteriaOperator.ToString();
            NewTeamControlManager.ModifyGridViewDataSourceSelectCommandFromSelectQuery(GridViewSqlDataSource,ParamSelectQuery);
            NewTeamGridView.DataBind();
            Session["SQL"] = GridViewSqlDataSource.SelectCommand;
            ResultQueryLabel.Text = Session["SQL"].ToString();
        }
    }
}
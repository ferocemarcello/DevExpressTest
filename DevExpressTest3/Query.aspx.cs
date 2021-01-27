using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Web.UI;
using DevExpress.XtraRichEdit.Layout;
using DevExpress.Data.Filtering;
using System.Web.Services;
using System.Xml.Linq;

namespace DevExpressTest3
{
    public partial class Query : System.Web.UI.Page/*, IPostBackEventHandler*/
    {
        /// <summary>
        /// metodo eseguito ad ogni caricamento della pagina. Tramite il parametro NAME della sessione ottiene(se non è null):
        /// l'elenco dei campi da parametrizzare della query selezionata.
        /// la descrizione della query selezionata.
        /// l'insieme delle tabelle, dei campi selezionati, dei filtri, dei raggruppamenti, e di tutti i parametri della query selezionata.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            hfselectedFields.Value = StringManager.GetStringOfParamFieldsFromXelement(DbManager.GetParameters((string)Session["NAME"]));
            hfDescription.Value = (Session["NAME"] == null ? "" : DbManager.GetQueryDescription((string)Session["NAME"]));
            if ((string)Session["NAME"] != null)
            {
                SelectQuery openConnectionSelectQuery = SelectQueryManager.DeserializeFromXml((string)Session["NAME"]);
                NewTeamQueryBuilder.OpenConnection("Tesis2002ConnectionString", openConnectionSelectQuery);
            }
            else NewTeamQueryBuilder.OpenConnection("Tesis2002ConnectionString");
        }
        /// <summary>
        /// Metodo che viene eseguito quando viene premuto il tasto save del query builder
        /// Imposta i parametri SQL e NAME della Session basandosi rispettivamente sul selectStatement del querybuilder, e sul nome assegnato alla query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ci fornisce molti parametri del querybuilder appena salvato, tra cui il nome ed il selectstament</param>
        protected void NewTeamQueryBuilder_SaveQuery(object sender, DevExpress.XtraReports.Web.SaveQueryEventArgs e)
        {
            
            if (EmptyQueryName(e.ResultQuery.Name)) return;
            Session["SQL"] = e.SelectStatement;
            if (DbManager.ExistingQuery(e.ResultQuery.Name)) DbManager.UpdateQuery(e.SelectStatement, e.ResultQuery.Name, hfDescription.Value == "" ? null : hfDescription.Value);
            else DbManager.WriteQuery(e.SelectStatement, e.ResultQuery.Name, hfDescription.Value == "" ? null : hfDescription.Value);
            XElement paramXml = null;
            if (hfFilterFields.Value != "")
            {
                string[] paramFields = hfFilterFields.Value.Split(';');
                paramXml = new XElement("StringList",paramFields.Select(x => new XElement("String", x + ";" + DbManager.GetColumnType(x).Name)));
            }
            DbManager.WriteParam(e.ResultQuery.Name, paramXml);
            SelectQueryManager.SerializeToXml(e.ResultQuery.Name,e.ResultQuery);
            Session["NAME"] = e.ResultQuery.Name;
            try
            {HttpContext.Current.Response.Redirect("~/Default.aspx");}
            catch (ApplicationException)
            {HttpContext.Current.Response.RedirectLocation = System.Web.VirtualPathUtility.ToAbsolute("~/Default.aspx");}
        }
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true se il textbox del querybuilder, contentenente il nome della query, è vuoto,altrimenti false</returns>
        private bool EmptyQueryName(string name)
        {
            var tables = DbManager.AllTables();
            if (tables.Contains(name.ToLower()))
            {
                MessageBox.Show("Type a query name", "Name Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }return false;
        }
    }
}
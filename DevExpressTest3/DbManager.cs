using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;

namespace DevExpressTest3
{
    public class DbManager
    {
        private static string connectionString;
        /// <summary>
        /// setta il campo connectionString, appartenente alla classe.
        ///In questo modo si facilita l'apertura ogni volta di connessioni,per letture o scritture
        /// </summary>
        static DbManager()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
            var connString = section.ConnectionStrings["Tesis2002ConnectionString"].ConnectionString;
            connectionString = connString;
        }

        /// <summary>
        /// </summary>
        /// <returns>l'elenco dei nomi di tutte le stringhe salvate</returns>
        public static List<string> GetQueryNames()
        {
            SqlConnection con = new SqlConnection(connectionString);
            string sql = "select Nome from Analytics.QuerySalvate";
            List<string> allNames = new List<string>();
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) allNames.Add(dr["Nome"].ToString());
            con.Close();
            return allNames;
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">nome della query di cui si vuole sapere la descrizione</param>
        /// <returns>la descrizione corrispondente al nome della query, passato come parametro</returns>
        public static string GetQueryDescription(string queryName)
        {
            queryName = StringManager.CorrectSingleQuoteString(queryName);
            SqlConnection con = new SqlConnection(connectionString);
            string sql = "select Descrizione from Analytics.QuerySalvate where Nome=" + "'" + queryName + "'";
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            dr.Read();
            string desc = dr["Descrizione"].ToString();
            con.Close();
            return desc;
        }
        /// <summary>
        /// </summary>
        /// <param name="name">il nome della query di cui ricercare l'esistenza</param>
        /// <returns>true se la query corrispondente al nome passato in input esiste, altrimenti false</returns>
        public static bool ExistingQuery(string name){return DbManager.GetQueryNames().Contains(name);}
        /// <summary>
        /// basato su una stringa di ddl o dml(quindi che modifica almeno un oggetto del db, senza restituire alcun valore), esegue quel comando
        /// </summary>
        /// <param name="command">la stringa che rappresenta il comando da eseguire</param>
        private static void voidCommand(string command)
        {
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = command;
            cmd.Connection = con;
            con.Open();
            cmd.BeginExecuteNonQuery();
            con.Close();
        }
        /// <summary>
        /// aggiorna i campi di testoquery e descrizione(valori passati in input) della query corrispondente al nome passato in input
        /// </summary>
        /// <param name="selectStatement">il testo dell'istruzione select che rimpiazzerà quello precedente</param>
        /// <param name="name">il nome della query su cui fare l'aggiornamento</param>
        /// <param name="description">la nuova descrizione della query</param>
        public static void UpdateQuery(string selectStatement, string name, string description)
        {
            selectStatement = StringManager.CorrectSingleQuoteString(selectStatement);
            name= StringManager.CorrectSingleQuoteString(name);
            description = StringManager.CorrectSingleQuoteString(description);
            voidCommand("UPDATE Analytics.QuerySalvate SET TestoQuery='" + selectStatement + "',Descrizione=" + (string.IsNullOrEmpty(description) ? "NULL" : ("'" + description + "'")) + " where Nome='" + name + "'");
        }
        /// <summary>
        /// Aggiorna il campo parametri della query il cui nome corrisponde a quello passato in input, con l'insieme dei parametri passati in input
        /// </summary>
        /// <param name="name">il nome della query su cui aggiornare i parametri</param>
        /// <param name="xmlParam">l'insieme dei parametri da scrivere</param>
        public static void WriteParam(string name, XElement xmlParam)
        {
            name = StringManager.CorrectSingleQuoteString(name);
            string xmlParamString= StringManager.CorrectSingleQuoteString(xmlParam.ToString());
            if (xmlParam != null)
                voidCommand("UPDATE Analytics.QuerySalvate SET Parametri='" + xmlParamString + "' where Nome='" + name + "'");
            else voidCommand("UPDATE Analytics.QuerySalvate SET Parametri=NULL where Nome='" + name + "'");
        }
        /// <summary>
        /// </summary>
        /// <param name="fullColumnName">è il [nomedelloschema.]nomedellatabella/vista.nomedelcampo</param>
        /// <returns>il tipo della colonna passata in input</returns>
        public static Type GetColumnType(string fullColumnName)
        {
            string cname=StringManager.GetColumnNameFromFullName(fullColumnName);
            string tname = StringManager.GetTableNameFromFullName(fullColumnName);
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT "+ cname+" FROM "+tname, con);
            con.Open();
            try
            {
                SqlDataReader rdr = cmd.ExecuteReader();
                Type t = rdr.GetFieldType(0);
                con.Close();
                return t;
            }
            catch (Exception)
            {
                con.Close();
                return null;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">il nome della query</param>
        /// <returns>il Xelement che corrisponde ai parametri della query in input</returns>
        public static System.Xml.Linq.XElement GetParameters(string queryName)
        {
            queryName = StringManager.CorrectSingleQuoteString(queryName);
            SqlConnection con = new SqlConnection(connectionString);
            string sql = "select Parametri from Analytics.QuerySalvate where Nome=" + "'" + queryName + "'";
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            string xml = null;
            if (dr.Read()) xml= dr["Parametri"].ToString();
            con.Close();
            var xelement = xml==""||xml==null?null:XElement.Parse(xml);
            return xelement;
        }
        /// <summary>
        /// </summary>
        /// <returns>una lista dei nomi di tutte le tabelle</returns>
        public static List<string> AllTables()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            DataTable schema = connection.GetSchema("Tables");
            List<string> TableNames = new List<string>();
            foreach (DataRow row in schema.Rows)
            {
                TableNames.Add(row[1].ToString().ToLower()+"_"+row[2].ToString().ToLower());
            }
            connection.Close();
            return TableNames;
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">il nome della query</param>
        /// <returns>l'oggetto XElement corrispondente al campo XmlQuery corrispondente al nome della query in input</returns>
        public static XElement GetSelectQueryAsXElementFromDb(string queryName)
        {
            queryName = StringManager.CorrectSingleQuoteString(queryName);

            SqlConnection con = new SqlConnection(connectionString);
            string sql = "select XmlQuery from Analytics.QuerySalvate where Nome=" + "'" + queryName + "'";
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                string xml = dr["XmlQuery"].ToString();
                con.Close();
                var xelement=XElement.Parse(xml);
                return xelement;
            }
            else
            {
                con.Close();
                return null;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">il nome della query su cui aggiornare il campo XmlQuery</param>
        /// <param name="xml">il campo XmlQuery(come stringa) da scrivere</param>
        public static void writeSelectQueryAsXml(string queryName,string xml)
        {
            queryName = StringManager.CorrectSingleQuoteString(queryName);
            xml = StringManager.CorrectSingleQuoteString(xml);
            voidCommand("UPDATE Analytics.QuerySalvate SET XmlQuery= " + "'" + xml + "'" + "where Nome=" + "'" + queryName + "'");
        }
        /// <summary>
        /// </summary>
        /// <param name="connectionString">la stringa di connessione da testare</param>
        /// <returns>true se ci si riesce a connettere con la determinata stringa di connessione, altrimenti false</returns>
        public static bool TestConnectionString(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">il nome della query da cui ottenere il selectstatement</param>
        /// <returns>il selectstatement corrispondente al nome della query in input</returns>
        public static string GetSqlFromQueryName(string queryName)
        {
            queryName = StringManager.CorrectSingleQuoteString(queryName);
            SqlConnection con = new SqlConnection(connectionString);
            string sql = "select TestoQuery from Analytics.QuerySalvate where Nome="+"'"+queryName+"'";
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader dr = cmd.ExecuteReader();
            string sqlRet = null;
            if (dr.Read()) sqlRet= dr["TestoQuery"].ToString();
            con.Close();
            return sqlRet;
        }
        /// <summary>
        /// </summary>
        /// <param name="selectStatement">il selectstatement da inserire</param>
        /// <param name="queryName">il nome della query da inserire</param>
        /// <param name="description">la descrizione della query da inserire</param>
        public static void WriteQuery(string selectStatement, string queryName, string description)
        {
            selectStatement = StringManager.CorrectSingleQuoteString(selectStatement);
            queryName = StringManager.CorrectSingleQuoteString(queryName);
            description = StringManager.CorrectSingleQuoteString(description);
            voidCommand("INSERT INTO Analytics.QuerySalvate (TestoQuery, Nome, Descrizione) VALUES (" + "'" + selectStatement + "'" + ",'" + queryName + "'" + "," + (string.IsNullOrEmpty(description) ? "NULL" : ("'" + description + "'")) + ")");
        }
        /// <summary>
        /// </summary>
        /// <returns>restituisce la stringa di connessione</returns>
        public static string GetConnectionString()
        {
            return connectionString;
        }
    }
}
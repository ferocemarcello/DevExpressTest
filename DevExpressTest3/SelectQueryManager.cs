using DevExpress.DataAccess.Native.Sql;
using DevExpress.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DevExpressTest3
{
    public class SelectQueryManager
    {
        /// <summary>
        /// Scrive nel campo XmlQuery in corrispondenza del nome della query in input, l'oggetto SelecQuery passato in input
        /// </summary>
        /// <param name="queryName">il nome della query sui cui scrivere in corrispondenza</param>
        /// <param name="resultQuery">l'oggetto SelectQuery da scrivere nel campo XmlQuery</param>
        public static void SerializeToXml(string queryName,SelectQuery resultQuery)
        {
            XElement queryXML = QuerySerializer.SaveToXml(resultQuery, null);
            string s = queryXML.ToString();
            DbManager.writeSelectQueryAsXml(queryName,s);
        }
        /// <summary>
        /// </summary>
        /// <param name="queryName">il nome della query da cui ottenere l'oggetto SelectQuery</param>
        /// <returns>Ottiene l'oggetto SelectQuery corrispondente al nome della query passato in input</returns>
        public static SelectQuery DeserializeFromXml(string queryName)
        {
            var query = QuerySerializer.LoadFromXml(DbManager.GetSelectQueryAsXElementFromDb(queryName), null) as SelectQuery;
            return query;
        }
    }
}
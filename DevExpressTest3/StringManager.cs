using System;
using System.Xml.Linq;

namespace DevExpressTest3
{
    public class StringManager
    {
        /// <summary>
        /// </summary>
        /// <param name="wrongString">la stringa da correggere</param>
        /// <returns>ritorna una nuova stringa, dove al posto di un single quote(') ci sono due single quote('')</returns>
        public static string CorrectSingleQuoteString(string wrongString)
        {
            if (String.IsNullOrEmpty(wrongString)) return null;
            string[] fragments = wrongString.Split('\'');
            string newString = "";
            foreach(string s in fragments)newString+= s + "''";
            newString =newString.Remove(newString.Length - 2);
            return newString;
        }
        /// <summary>
        /// </summary>
        /// <param name="fullColumnName">[nomedelloschema.]nomedellatabella/vista.nomedellacolonna</param>
        /// <returns>[nomedelloschema.]nomedellatabella/vista</returns>
        public static string GetTableNameFromFullName(string fullColumnName)
        {
            string[] ar = fullColumnName.Split('.');
            string tab = "";
            for (int i = 0; i < ar.Length; i++)if (i < ar.Length - 1) tab += ar[i]+".";
            tab = tab.Remove(tab.Length - 1);
            return tab;
        }
        /// <summary>
        /// </summary>
        /// <param name="fullColumnName">[nomedelloschema.]nomedellatabella/vista.nomedellacolonna</param>
        /// <returns>nomedellacolonna</returns>
        public static string GetColumnNameFromFullName(string fullColumnName)
        {
            string[] ar = fullColumnName.Split('.');
            return ar[ar.Length - 1];
        }
        /// <summary>
        /// </summary>
        /// <param name="paramXElement">oggetto XElement corrispondente a una cella della colonna parametri</param>
        /// <returns>stringa che concatena i parametri</returns>
        public static string GetStringOfParamFieldsFromXelement(XElement paramXElement)
        {
            if (paramXElement == null) return null;
            string s = "";
            foreach(var n in paramXElement.Nodes()) s += n.ToString().Split(';')[0].Substring(8) + ";";
            s = s.Remove(s.Length-1); return s;
        }
    }
}
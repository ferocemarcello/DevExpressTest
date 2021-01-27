using System;
using System.Web.UI.WebControls;
using DevExpress.Web;
using System.Web;
using System.Xml.Linq;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess.Sql;
using System.Web.UI;
using System.Collections.Generic;
using System.Collections;

namespace DevExpressTest3
{
    public class NewTeamControlManager
    {
        private const string dontuseoperand = "don't use this parameter";
        private const string isnullString = "null";
        private const string isnotnullString = "not null";
        private const string betweenString="between";
        private const string andBetweenString="and";
        /// <summary>
        /// </summary>
        /// <param name="visible">solo se è true viene modificato il testo del textbox</param>
        /// <param name="radioButtonQueryList">la lista delle query da cui prendere il nome della query selezionata</param>
        /// <param name="fileNameTextBox">il textbox di cui cambiare il testo</param>
        public static void AssignTextBoxFileName(bool visible, ASPxRadioButtonList radioButtonQueryList, TextBox fileNameTextBox)
        {
            if (visible)
            {
                if (radioButtonQueryList.SelectedItem == null) fileNameTextBox.Text = (string)HttpContext.Current.Session["NAME"];
                else fileNameTextBox.Text = radioButtonQueryList.SelectedItem.Text;
            }
        }
        /// <summary> 
        /// </summary>
        /// <param name="visible">boolean che se è true inposta i controlli visibili, altrimenti false</param>
        /// <param name="controls">i controlli da rendere visibili o invisibili</param>
        public static void SetControlsVisible(bool visible,params System.Web.UI.Control[] controls)
        {
            foreach (var c in controls) c.Visible = visible;
        }
        /// <summary>
        /// </summary>
        /// <param name="paramsXelement">l'oggetto XElement contenente tutti i parametri</param>
        /// <param name="ParamTable">la tabella su cui costruire tutti i controlli necessari per impostare i valori dei parametri</param>
        public static void BuildParamTable(XElement paramsXelement,System.Web.UI.WebControls.Table ParamTable)
        {
            foreach (var paramNode in paramsXelement.Nodes())
            {
                TableCell ColumnNameLabelTableCell = new TableCell();
                string columnName = paramNode.ToString().Split(';')[0].Substring(8);
                ColumnNameLabelTableCell.Controls.Add(new System.Web.UI.WebControls.Label { Text = columnName });
                ListItemCollection operatorsItems = new ListItemCollection
                {new ListItem() {Text="=" },new ListItem() {Text="<>" },new ListItem() {Text="<" },new ListItem() {Text="<=" },new ListItem() {Text=">" },new ListItem() {Text=">=" },new ListItem() {Text=isnotnullString },new ListItem() {Text=isnullString },new ListItem() {Text=dontuseoperand }};
                DropDownList operators = new DropDownList();
                string columnTypeString = paramNode.ToString().Split(';')[1].Remove(paramNode.ToString().Split(';')[1].LastIndexOf('<'));
                operators.SelectedIndexChanged += delegate (object sender, EventArgs e)
                { AddOrDeleteCellsBasingOnTheOperator(sender, e, ParamTable); };
                operators.AutoPostBack = true;
                foreach (ListItem li in operatorsItems) operators.Items.Add(li);
                TableCell OperatorTypeTableCell = new TableCell();
                OperatorTypeTableCell.Controls.Add(operators);
                TableCell paramControlTypeTableCell = new TableCell();
                FilterOperandList(columnTypeString, operatorsItems);
                paramControlTypeTableCell.Controls.Add(GetCustomizedControlFromDbType(columnTypeString));
                System.Web.UI.WebControls.TableRow tr = new System.Web.UI.WebControls.TableRow();
                tr.Cells.Add(ColumnNameLabelTableCell); tr.Cells.Add(OperatorTypeTableCell); tr.Cells.Add(paramControlTypeTableCell);
                ParamTable.Rows.Add(tr);
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="columnTypeString">stringa che rappresenta il tipo di dato di una colonna</param>
        /// <param name="operatorsItems">Collezione di ListItem(oggetti di DropDownList) di cui modificare la visibilità di alcuni elementi</param>
        private static void FilterOperandList(string columnTypeString, ListItemCollection operatorsItems)
        {
            if (columnTypeString.ToLower().Contains("bool"))
            {
                operatorsItems.FindByText("<").Enabled = false;
                operatorsItems.FindByText(">").Enabled = false;
                operatorsItems.FindByText("<=").Enabled = false;
                operatorsItems.FindByText(">=").Enabled = false;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="columnTypeString">stringa che rappresenta il tipo di dato di una colonna</param>
        /// <returns>restituisce un controllo che può ospitare un certo tipo di dato</returns>
        private static Control GetCustomizedControlFromDbType(string columnTypeString)
        {
            Control c;
            if (columnTypeString.ToLower().Contains("bool"))
            {
                c = new DropDownList { Items = { new ListItem() { Text = "true" }, new ListItem() { Text = "false" } } };
            }
            else if (columnTypeString.ToLower().Contains("date"))
            {
                c = new ASPxDateEdit();
                (c as ASPxDateEdit).NullText = "insert a proper date";
            }
            else if (columnTypeString.ToLower().Contains("time"))
            {
                c = new ASPxTimeEdit();
                (c as ASPxTimeEdit).NullText = "insert a proper time";
            }
            else if (columnTypeString.ToLower().Contains("int") || columnTypeString.ToLower().Contains("decimal") || columnTypeString.ToLower().Contains("float"))
            {
                c = new ASPxSpinEdit();
                (c as ASPxSpinEdit).SpinButtons.ShowIncrementButtons = true;
                (c as ASPxSpinEdit).SpinButtons.ShowLargeIncrementButtons = true;
                if (columnTypeString.ToLower().Contains("int") || columnTypeString.ToLower().Contains("decimal"))
                {
                    (c as ASPxSpinEdit).NumberType = SpinEditNumberType.Integer; (c as ASPxSpinEdit).NullText = "insert a proper integer";
                }
                else { (c as ASPxSpinEdit).NumberType = SpinEditNumberType.Float; (c as ASPxSpinEdit).NullText = "insert a proper float"; }
            }
            else
            {
                c = new ASPxTextBox();
            }return c;
        }
        /// <summary>
        /// se si richiede che un campo si null o not null, la cella che di solito è dedicata al valore del secondo operando, viene eliminata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="ParamTable">la tabella dei parametri da scorrere</param>
        private static void AddOrDeleteCellsBasingOnTheOperator(object sender, EventArgs e, System.Web.UI.WebControls.Table ParamTable)
        {
            for (int rowindex = 0; rowindex < ParamTable.Rows.Count; rowindex++)
            {
                if ((ParamTable.Rows[rowindex].Cells[1].Controls[0] as DropDownList).SelectedItem.Text == isnullString || (ParamTable.Rows[rowindex].Cells[1].Controls[0] as DropDownList).SelectedItem.Text == isnotnullString || (ParamTable.Rows[rowindex].Cells[1].Controls[0] as DropDownList).SelectedItem.Text == dontuseoperand)
                {
                    ParamTable.Rows[rowindex].Cells.RemoveAt(2);
                }
            }      
        }
        /// <summary>
        /// </summary>
        /// <param name="paramNodes">elenco dei parametri come IEnumerable di XNode</param>
        /// <param name="rowindex">l'indice sull'elenco dei parametri sul quale indagare</param>
        /// <returns>il tipo della colonna corrispondente, come stringa</returns>
        private static string GetColumnTypeFromParamAndIndex(IEnumerable<XNode> paramNodes, int rowindex)
        {
            IEnumerator<XNode> en = paramNodes.GetEnumerator();
            for(int i = 0; i <= rowindex; i++)
            {
                en.MoveNext();
            }return en.Current.ToString().Split(';')[1].Remove(en.Current.ToString().Split(';')[1].LastIndexOf('<'));
        }
        /// <summary>
        /// basandosi su un oggetto di tipo SelectQuery, si ottiene l'sql corrispondente e lo si assegna ad una data source
        /// </summary>
        /// <param name="gridViewSqlDataSource">la data source di cui cambiare il selectcommand</param>
        /// <param name="paramSelectQuery">l'oggetto selectquery da cui calcolare il sele</param>
        public static void ModifyGridViewDataSourceSelectCommandFromSelectQuery(System.Web.UI.WebControls.SqlDataSource gridViewSqlDataSource, SelectQuery paramSelectQuery)
        {
            DevExpress.DataAccess.Sql.SqlDataSource fakeSqlDataSource = new DevExpress.DataAccess.Sql.SqlDataSource("Tesis2002ConnectionString");
            fakeSqlDataSource.Queries.Add(paramSelectQuery);
            fakeSqlDataSource.Connection.Open();
            gridViewSqlDataSource.SelectCommand = paramSelectQuery.GetSql(fakeSqlDataSource.Connection.GetDBSchema());
            fakeSqlDataSource.Connection.Close();
        }
        /// <summary>
        /// </summary>
        /// <param name="op">oggetto CriteriaOperator che costituisce la filter string</param>
        /// <param name="ParamTable">Tabella dei parametri da cui prendere i valori per costruire la nuova filtersting come CriteriaOperator</param>
        /// <returns>nuovo oggetto criteriaoperator che rappresenta la nuova filterstring</returns>
        public static CriteriaOperator GetCriteriaOperatorFromPreviousValueAndParamTable(CriteriaOperator op, System.Web.UI.WebControls.Table ParamTable)
        {
            for (int x = 0; x < ParamTable.Rows.Count; x++)
            {
                switch((ParamTable.Rows[x].Cells[1].Controls[0] as DropDownList).SelectedItem.Text)
                {
                    case dontuseoperand:break;
                    case isnullString:
                    case isnotnullString:
                        {
                            try
                            {
                                if ((ParamTable.Rows[x].Cells[1].Controls[0] as DropDownList).SelectedItem.Text == isnullString) (op as GroupOperator).Operands.Add(new UnaryOperator(UnaryOperatorType.IsNull, new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text)));
                                else (op as GroupOperator).Operands.Add(new NotOperator(new UnaryOperator(UnaryOperatorType.IsNull, new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text))));
                            }
                            catch (NullReferenceException)
                            {
                                GroupOperator go = new GroupOperator(new CriteriaOperator[0]);
                                go.Operands.Add(op);
                                if ((ParamTable.Rows[x].Cells[1].Controls[0] as DropDownList).SelectedItem.Text == isnullString)
                                    go.Operands.Add(new UnaryOperator(UnaryOperatorType.IsNull, new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text)));
                                else go.Operands.Add(new NotOperator(new UnaryOperator(UnaryOperatorType.IsNull, new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text))));
                                op = go as CriteriaOperator;
                            }
                            break;
                        }
                    default:
                        {
                            var c = ParamTable.Rows[x].Cells[2].Controls[0];
                            ConstantValue cv = new ConstantValue();
                            cv = GetConstantValueFromParamControl(c);
                            try
                            {
                                (op as GroupOperator).Operands.Add(
                               new BinaryOperator(
                               new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text),
                               cv,
                               GetBinaryOperatorTypeFromText((ParamTable.Rows[x].Cells[1].Controls[0] as DropDownList).Text)
                               )
                               );
                            }
                            catch (NullReferenceException)
                            {
                                GroupOperator go = new GroupOperator(new CriteriaOperator[0]);
                                go.Operands.Add(op);
                                go.Operands.Add(
                                new BinaryOperator(
                                new OperandProperty((ParamTable.Rows[x].Cells[0].Controls[0] as System.Web.UI.WebControls.Label).Text),
                                cv,
                                GetBinaryOperatorTypeFromText((ParamTable.Rows[x].Cells[1].Controls[0] as DropDownList).Text)
                                )
                                );
                                op = go as CriteriaOperator;
                            }
                            break;
                        }
                }
            }
            return op;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">testo che rapressenta l'operatore</param>
        /// <returns>true se l'operatore è di tipo unitario(null o not null)</returns>
        private static bool IsUnitaryOperand(string text) {return (text.ToLower() == isnullString || text.ToLower() == isnotnullString);}
        /// <summary>
        /// </summary>
        /// <param name="paramControl">controllo che contiene il valore di un parametro inserito da un utente</param>
        /// <returns>restituisce il contenuto del controllo come ConstantValue</returns>
        private static ConstantValue GetConstantValueFromParamControl(System.Web.UI.Control paramControl)
        {
            if (paramControl.GetType() == typeof(DropDownList))
            {
                return new ConstantValue((paramControl as DropDownList).SelectedItem.Text == "true" ? true : false);
            }
            if (paramControl.GetType() == typeof(ASPxDateEdit))
            {
                return ((paramControl as ASPxDateEdit).Text == "") ? new ConstantValue(new DateTime(1753, 1, 1, 0, 0, 0)):new ConstantValue((paramControl as ASPxDateEdit).Date);
            }
            if (paramControl.GetType() == typeof(ASPxTimeEdit))
            {
                return new ConstantValue((paramControl as ASPxDateEdit).Date.TimeOfDay);
            }
            if (paramControl.GetType() == typeof(ASPxSpinEdit))
            {
                return ((paramControl as ASPxSpinEdit).Text == "")? new ConstantValue(0): new ConstantValue((paramControl as ASPxSpinEdit).Number.ToString());
            }
            if (paramControl.GetType() == typeof(ASPxTextBox))
            {
                return new ConstantValue((paramControl as ASPxPureTextBoxBase).Text);
            }
            return null;
        }
        /// <summary>
        /// </summary>
        /// <param name="text">testo dell'operatore</param>
        /// <returns>un oggetto di tipo BinaryOperatorType, che rappresenta il tipo di operatore binario</returns>
        private static BinaryOperatorType GetBinaryOperatorTypeFromText(string text)
        {
            switch (text)
            {
                case ("="): return BinaryOperatorType.Equal;
                case ("<"): return BinaryOperatorType.Less;
                case ("<="): return BinaryOperatorType.LessOrEqual;
                case (">"): return BinaryOperatorType.Greater;
                case (">="): return BinaryOperatorType.GreaterOrEqual;
                case ("<>"): return BinaryOperatorType.NotEqual;
                default: return BinaryOperatorType.Equal;
            }
        }
    }
}
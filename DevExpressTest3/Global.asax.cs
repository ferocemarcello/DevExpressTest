using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using DevExpress.Web;
using DevExpress.XtraReports.Web.QueryBuilder;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Sql;
using DevExpress.Xpo.DB;

namespace DevExpressTest3 {
        public class Global_asax : System.Web.HttpApplication {
            void Application_Start(object sender, EventArgs e) {
            
                DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
            //istruzione che serve per limitare le tabelle, procedure, o viste disponibili al query builder
            DefaultQueryBuilderContainer.Register<IDataSourceWizardDBSchemaProviderExFactory, MyDataSourceWizardDBSchemaProviderFactory>();
        }
        private class MyDataSourceWizardDBSchemaProviderFactory : IDataSourceWizardDBSchemaProviderExFactory
        {
            public IDBSchemaProviderEx Create()
            {
                return new MyDBSchemaProvider();
            }

            private class MyDBSchemaProvider : IDBSchemaProviderEx
            {
                public DBStoredProcedure[] GetProcedures(SqlDataConnection connection, params string[] procedureList)
                {
                    DBSchema defaultSchema = connection.GetDBSchema(false, SchemaLoadingMode.StoredProcedures);
                    DBStoredProcedure[] storedProcedures = defaultSchema.StoredProcedures.Where((storedProcedure) => { return storedProcedure.Arguments.Count == 0; }).ToArray();
                    return storedProcedures;
                }

                public DBTable[] GetTables(SqlDataConnection connection, params string[] tableList)
                {
                    /*DBSchema defaultSchema = connection.GetDBSchema(false);
                    DBTable[] tables = defaultSchema.Tables.Where((table) => { return table.Name.StartsWith("A"); }).ToArray();
                    return tables;*/
                    return new DBTable[0];
                }

                public DBTable[] GetViews(SqlDataConnection connection, params string[] viewList)
                {
                    DBSchema defaultSchema = connection.GetDBSchema(false);
                    DBTable[] views = defaultSchema.Views.Where((view) => { return view.Name.StartsWith("Analytics."); }).ToArray();
                    return views;
                }

                public void LoadColumns(SqlDataConnection connection, params DBTable[] tables)
                {
                    connection.LoadDBColumns(tables);
                }
            }
        }
        void Application_End(object sender, EventArgs e) {
                // Code that runs on application shutdown
            }

            void Application_Error(object sender, EventArgs e) {
                // Code that runs when an unhandled error occurs
            }

            void Session_Start(object sender, EventArgs e) {
            // Code that runs when a new session is started
        }

            void Session_End(object sender, EventArgs e) {
                // Code that runs when a session ends. 
                // Note: The Session_End event is raised only when the sessionstate mode
                // is set to InProc in the Web.config file. If session mode is set to StateServer 
                // or SQLServer, the event is not raised.
            }
    }
    }
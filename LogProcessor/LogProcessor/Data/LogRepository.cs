using LogProcessor.LogProcessor;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;
using System.Data;
using System;

namespace LogProcessor.Data
{
    public class LogRepository : ILogRepository
    {
        private const string TABLE_NAME = "dbo.app_performance_lucas";        
        private const string DATE_COLUMN_NAME = "Date";
        private const string IP_COLUMN_NAME = "IP";
        private const string URL_COLUMN_NAME = "URL";
        private const string CONNECTION_NAME = "DatabaseConnection";

        public void Save(List<LogItemContract> items)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings[CONNECTION_NAME].ConnectionString);
            SqlTransaction t = null;

            try
            {
                con.Open();
                t = con.BeginTransaction();

                var lockDataTable = new object();
                var dataTable = new DataTable(TABLE_NAME);

                var dateColumn = new DataColumn(DATE_COLUMN_NAME, typeof(DateTime));
                var ipColumn = new DataColumn(IP_COLUMN_NAME, typeof(string));
                var urlColumn = new DataColumn(URL_COLUMN_NAME, typeof(string));                

                dataTable.Columns.Add(ipColumn);
                dataTable.Columns.Add(urlColumn);
                dataTable.Columns.Add(dateColumn);
                
                Parallel.ForEach(items, (LogItemContract item) =>
                {
                    var row = dataTable.NewRow();
                    row[DATE_COLUMN_NAME] = item.Date;
                    row[IP_COLUMN_NAME] = item.IP;
                    row[URL_COLUMN_NAME] = item.URL;

                    lock (lockDataTable) 
                        dataTable.Rows.Add(row);
                });

                using (var bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, t))
                {   
                    bulkCopy.DestinationTableName = TABLE_NAME;
                    bulkCopy.ColumnMappings.Add(DATE_COLUMN_NAME, DATE_COLUMN_NAME);
                    bulkCopy.ColumnMappings.Add(IP_COLUMN_NAME, IP_COLUMN_NAME);
                    bulkCopy.ColumnMappings.Add(URL_COLUMN_NAME, URL_COLUMN_NAME);

                    bulkCopy.WriteToServer(dataTable);
                }

                t.Commit();
            }
            catch(Exception ex)
            {
                t.Rollback();
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }
    }
}

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
        public void Save(List<LogItemContract> items)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString);
            SqlTransaction t = null;

            try
            {
                con.Open();
                t = con.BeginTransaction();

                var lockDataTable = new object();
                var dataTable = new DataTable("dbo.app_performance_lucas");

                var dateColumn = new DataColumn("Date", typeof(DateTime));
                var ipColumn = new DataColumn("IP", typeof(string));
                var urlColumn = new DataColumn("URL", typeof(string));                

                dataTable.Columns.Add(ipColumn);
                dataTable.Columns.Add(urlColumn);
                dataTable.Columns.Add(dateColumn);

                //foreach (var item in items)
                //{
                //    var row = dataTable.NewRow();
                //    row["Date"] = item.Date;
                //    row["IP"] = item.IP;
                //    row["URL"] = item.URL;                    

                //    dataTable.Rows.Add(row);
                //}

                Parallel.ForEach(items, (LogItemContract item) =>
                {
                    var row = dataTable.NewRow();
                    row["Date"] = item.Date;
                    row["IP"] = item.IP;
                    row["URL"] = item.URL;

                    lock (lockDataTable) 
                        dataTable.Rows.Add(row);
                });

                using (var bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, t))
                {   
                    bulkCopy.DestinationTableName = "dbo.app_performance_lucas";
                    bulkCopy.ColumnMappings.Add("Date", "Date");
                    bulkCopy.ColumnMappings.Add("IP", "IP");
                    bulkCopy.ColumnMappings.Add("URL", "URL");

                    bulkCopy.WriteToServer(dataTable);
                }

                //Parallel.ForEach(items, (LogItemContract item) =>
                //{
                //    var command = con.CreateCommand();
                //    command.Transaction = t;
                //    command.CommandText = "INSERT INTO [dbo].[app_performance_lucas](Date, IP, URL) VALUES (@Date, @IP, @URL)";
                //    command.Parameters.AddRange(GetParameters(item));

                //    command.ExecuteNonQuery();
                //});

                t.Commit();
            }
            catch (System.Exception ex)
            {
                t.Rollback();
            }
            finally
            {
                con.Close();
            }
        }

        private SqlParameter[] GetParameters(LogItemContract item)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@Date", item.Date));
            parameters.Add(new SqlParameter("@IP", item.IP));
            parameters.Add(new SqlParameter("@URL", item.URL));

            return parameters.ToArray();
        }
    }
}

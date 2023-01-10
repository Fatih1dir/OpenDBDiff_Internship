using OpenDBDiff.Abstractions.Schema;
using OpenDBDiff.Abstractions.Schema.Model;
using OpenDBDiff.Abstractions.Ui;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;


namespace OpenDBDiff.SqlServer.Schema.Options
{
    public class SqlOptionFilter : IOptionFilter
    {
        public SqlOptionFilter()
        {
            Items = new List<SqlOptionFilterItem>
            {
                new SqlOptionFilterItem(ObjectType.Table, "dbo.dtproperties"),
                new SqlOptionFilterItem(ObjectType.Assembly, "Microsoft.SqlServer.Types"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_accessadmin"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_backupoperator"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_datareader"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_datawriter"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_ddladmin"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_denydatareader"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_denydatawriter"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_owner"),
                new SqlOptionFilterItem(ObjectType.Schema, "db_securityadmin"),
                //new SqlOptionFilterItem(ObjectType.Schema, "dbo"),
                new SqlOptionFilterItem(ObjectType.Schema, "guest"),
                new SqlOptionFilterItem(ObjectType.Schema, "INFORMATION_SCHEMA"),
                new SqlOptionFilterItem(ObjectType.Schema, "sys")

            };

            //string connectionString = DestinationNSource.destinationNSource.RightDatabaseSelector.ConnectionString;
            string connectionString = DestinationNSource.rigthconString;
            if (connectionString != "Data Source=(local);Integrated Security=True" && connectionString !=null)
            {
                //Console.WriteLine(DestinationNSource.rigthconString);
                int startIndex = connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                int endIndex = connectionString.IndexOf(";", startIndex);
                string databaseName = connectionString.Substring(startIndex, endIndex - startIndex);

                int startIndex1 = connectionString.IndexOf("Data Source=") + "Data Source=".Length;
                int endIndex1 = connectionString.IndexOf(";", startIndex1);
                if (endIndex1 == -1)
                {
                    endIndex1 = connectionString.Length;
                }
                string serverName = connectionString.Substring(startIndex1, endIndex1 - startIndex1);

                //string connectionString1 = "Server=ENFOTEK75\\SQLEXPRESS01;Database=TESTDATA2;Integrated Security=SSPI";
                string connectionString1 = "Server=" + serverName + ";Database=" + databaseName + ";Integrated Security=SSPI";

                string sql = "SELECT AID_UPDATE_EXCEPTION FROM AID_UPDATE_EXCEPTIONS";




                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string sqlTypeSearch = "SELECT type, name FROM sys.objects WHERE name=\'" + reader.GetString(0) + "\'";
                                    string type = "";
                                    using (SqlConnection connection2 = new SqlConnection(connectionString))
                                    {
                                        connection2.Open();

                                        using (SqlCommand command1 = new SqlCommand(sqlTypeSearch, connection2))
                                        {
                                            using (SqlDataReader reader1 = command1.ExecuteReader())
                                            {
                                                while (reader1.Read())
                                                {
                                                    string objectSearchType = reader1.GetString(0).Trim();
                                                    if (objectSearchType.Equals("P"))
                                                        type = "StoredProcedure";
                                                    else if (objectSearchType.Equals("V"))
                                                        type = "View";
                                                    else if (objectSearchType.Equals("TR"))
                                                        type = "Trigger";
                                                    else if (objectSearchType.Equals("IF") || objectSearchType.Equals("FN") || objectSearchType.Equals("TF") || objectSearchType.Equals("FS"))
                                                        type = "Function";
                                                }
                                            }
                                        }
                                    }
                                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), type, true);
                                    string filterPattern = reader.GetString(0);

                                    Items.Add(new SqlOptionFilterItem(objectType, filterPattern));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Exception Table does not exist in the destination database!");
                    }
                    
                }
            }
            
        }

        public SqlOptionFilter(IOptionFilter optionFilter)
        {
            Items = new List<SqlOptionFilterItem>();
            var options = optionFilter.GetOptions();
            foreach (var pair in options)
            {
                Items.Add(
                    new SqlOptionFilterItem(
                        objectType: (ObjectType)Enum.Parse(typeof(ObjectType), pair.Value, true),
                        filterPattern: pair.Key
                    )
                );
                
            };
        }

        public IList<SqlOptionFilterItem> Items { get; private set; }

        public IDictionary<string, string> GetOptions()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            for (int i = 0; i < Items.Count; i++)
            {
                values.Add(Items[i].FilterPattern, Items[i].ObjectType.ToString());
            }
            return values;
        }

        public bool IsItemIncluded(ISchemaBase item)
        {
            return !Items.Any(i => i.IsMatch(item));
        }
    }
}

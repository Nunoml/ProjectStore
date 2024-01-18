using Microsoft.Data.SqlClient;

namespace ProjectStore.Files.DataAccess
{
    public class DBConnection
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        protected SqlConnection connect;

        public DBConnection(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            connect = new SqlConnection(connectionString);
        }

        public bool IsAvailable()
        {
            try
            {
                connect.Open();
                connect.Close();
            }
            catch(SqlException)
            {
                return false;
            }
            return true;
        }
        public string RunQuery(string query)
        {
            string output = "";
            try
            {
                connect.Open();
                using (SqlCommand comm = new SqlCommand(query, connect))
                {
                    using (SqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Object[] values = new Object[reader.FieldCount];
                            int fieldCount = reader.GetValues(values);
                            for(int i = 0; i < fieldCount; i++) 
                            {
                                output += values[i] + " ";
                            }
                            output += "\n";
                            
                        }
                        reader.Close();
                    }
                    comm.Dispose();
                }
                connect.Close();
            }
            catch(SqlException)
            {
                return "";
            }
            return output;
        }

    }
}

using ProjectStore.Files.DataAccess.Model;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ProjectStore.Files.DataAccess
{
    public class FileTable : DBConnection
    {
        public FileTable(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
            // Verificar se a tabela existe, se não existir, criar uma
            try
            {
                connect.Open();
                // ??? TODO: TESTAR
                string query = "IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'File') CREATE TABLE File(Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY, UserId INT, FileName VARCHAR(255), Inside INT)";
                using ( SqlCommand command = new SqlCommand(query, connect))
                {
                       command.ExecuteNonQuery();
                       command.Dispose();
                }
                connect.Close();
                
            }
            catch(SqlException)
            {
                Console.WriteLine("Error running critical query! Exiting...");
                System.Environment.Exit(1);
            }
        }

        public List<FileModel> GetAll(string selectQuery)
        {
            List<FileModel> FileList = new List<FileModel>();
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(selectQuery, connect))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        FileModel fileToAdd = new FileModel
                        {
                            Id = (int)reader["Id"],
                            UserId = (int)reader["UserId"],
                            FileName = (string)reader["FileName"],
                            Inside = (int)reader["Path"]
                        };

                        FileList.Add(fileToAdd);
                    }
                    command.Dispose();
                }
                connect.Close();
            }
            catch(SqlException)
            {
                Console.WriteLine("Error trying to run query");
                return null;
            }
            return FileList;
        }

        public FileModel Get(string selectQuery)
        {
            FileModel fileToAdd = new FileModel();
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(selectQuery, connect))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    fileToAdd = new FileModel
                    {
                        Id = (int)reader["Id"],
                        UserId = (int)reader["UserId"],
                        FileName = (string)reader["FileName"],
                        Inside = (int)reader["Inside"]
                    };

                    command.Dispose();
                }
                connect.Close();
            }
            catch (SqlException)
            {
                Console.WriteLine("Error trying to run query");
                return null;
            }
            return fileToAdd;
        }
        public bool Insert(FileModel table)
        {
            string query = $"INSERT INTO File (Id, UserId, FileName, Inside) VALUES ({table.Id}, {table.UserId}, {table.FileName}, {table.Inside})";
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(query, connect))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connect.Close();
            }
            catch(SqlException)
            {
                return false;
            }
            
            return true;
        }
        public bool Update(FileModel table)
        {
            string query = $"UPDATE File SET FileName = {table.FileName}, Inside={table.Inside} WHERE Id={table.Id}";
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(query, connect))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connect.Close();
            }
            catch (SqlException)
            {
                return false;
            }

            return true;
        }
        public bool Delete(FileModel table)
        {
            // Apagar os dados, não existe anonimização, como esta-se a trabalhar com entradas de ficheiros.
            string query = $"DELETE FROM File WHERE Id={table.Id}";
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(query, connect))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connect.Close();
            }
            catch (SqlException)
            {
                return false;
            }

            return true;
        }
    }
}
using Microsoft.Data.SqlClient;
using ProjectStore.Files.DataAccess.Model;
using System;

namespace ProjectStore.Files.DataAccess
{
    public class DirectoryTable : DBConnection
    {
        public DirectoryTable(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
            // Verificar se a tabela existe, se não existir, criar uma
            try
            {
                connect.Open();
                // ??? TODO: TESTAR
                string query = "IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'Directory') CREATE TABLE Directory(Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY, UserId INT, DirName VARCHAR(255), Inside INT)";
                using (SqlCommand command = new SqlCommand(query, connect))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connect.Close();

            }
            catch (SqlException)
            {
                Console.WriteLine("Error running critical query! Exiting...");
                System.Environment.Exit(1);
            }
        }

        public List<DirectoryModel> GetAll(string selectQuery)
        {
            List<DirectoryModel> FileList = new List<DirectoryModel>();
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(selectQuery, connect))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DirectoryModel fileToAdd = new DirectoryModel
                        {
                            Id = (int)reader["Id"],
                            UserId = (int)reader["UserId"],
                            DirName = (string)reader["FileName"],
                            Inside = (int)reader["Inside"]
                        };

                        FileList.Add(fileToAdd);
                    }
                    command.Dispose();
                }
                connect.Close();
            }
            catch (SqlException)
            {
                Console.WriteLine("Error trying to run query");
                return null;
            }
            return FileList;
        }

        public DirectoryModel Get(string selectQuery)
        {
            DirectoryModel fileToAdd = new DirectoryModel();
            try
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand(selectQuery, connect))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    fileToAdd = new DirectoryModel
                    {
                        Id = (int)reader["Id"],
                        UserId = (int)reader["UserId"],
                        DirName = (string)reader["FileName"],
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
        public bool Insert(DirectoryModel table)
        {
            string query = $"INSERT INTO File (Id, UserId, FileName, Inside) VALUES ({table.Id}, {table.UserId}, {table.DirName}, {table.Inside})";
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
        public bool Update(DirectoryModel table)
        {
            string query = $"UPDATE File SET FileName = {table.DirName}, Inside={table.Inside} WHERE Id={table.Id}";
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
        public bool Delete(DirectoryModel table)
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

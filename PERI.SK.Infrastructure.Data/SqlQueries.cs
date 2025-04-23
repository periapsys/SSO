using System.Text;
using PERI.SK.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace PERI.SK.Infrastructure.Data
{
    public class SqlQueries : IDataQueries
    {
        public async Task<bool> CanConnect(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection and check its state
                    await connection.OpenAsync();
                    return connection.State == System.Data.ConnectionState.Open;
                }
                catch (SqlException ex)
                {
                    // Handle exception (e.g., network, authentication issues)
                    Console.WriteLine($"Connection failed: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<string> GetFields(string connectionString, string schema, string table)
        {
            var resultBuilder = new StringBuilder();

            var sql = @$"SELECT 
    TABLE_SCHEMA + '.' + TABLE_NAME AS Table_Name, 
    COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '{table}'
AND TABLE_SCHEMA = '{schema}'";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Loop through all records and append them to resultBuilder
                        while (await reader.ReadAsync())
                        {
                            // For each row, append all columns to the result
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                resultBuilder.Append(reader[i].ToString());

                                // Add a separator between columns (e.g., comma)
                                if (i < reader.FieldCount - 1)
                                {
                                    resultBuilder.Append(", ");
                                }
                            }

                            // Move to the next line after each row
                            resultBuilder.AppendLine();
                        }
                    }
                }
            }

            return resultBuilder.ToString();
        }

        public async Task<string> GetData(string connectionString, string? query = null)
        {
            var resultBuilder = new StringBuilder();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query!, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Loop through all records and append them to resultBuilder
                        while (await reader.ReadAsync())
                        {
                            // For each row, append all columns to the result
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                resultBuilder.Append(reader[i].ToString());

                                // Add a separator between columns (e.g., comma)
                                if (i < reader.FieldCount - 1)
                                {
                                    resultBuilder.Append(", ");
                                }
                            }

                            // Move to the next line after each row
                            resultBuilder.AppendLine();
                        }
                    }
                }
            }

            return resultBuilder.ToString();
        }
    }
}

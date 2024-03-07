using ConvertorToDataBase.Enums;
using ConvertorToDataBase.Exceptions;
using ConvertorToDataBase.Models;
using Npgsql;
using Npgsql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Modules
{
    internal class PostgresDataBaseManager : DatabaseManager
    {
        private readonly NpgsqlConnection _npgsqlConnection;
        private CultureInfo provider = CultureInfo.InvariantCulture;

        public PostgresDataBaseManager(NpgsqlConnection npgsqlConnection)
        {
            _npgsqlConnection = npgsqlConnection;
        }

        public override async Task OpenConnection() => await _npgsqlConnection.OpenAsync();

        public override async Task CloseConnection()
        {
            if (_npgsqlConnection.State != ConnectionState.Closed)
            {
                await _npgsqlConnection.CloseAsync();
            }
        }

        public override async Task<bool> TestConnection()
        {
            try
            {
                await _npgsqlConnection.OpenAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // Ensure that the connection is closed, whether an exception occurred or not
                if (_npgsqlConnection.State != ConnectionState.Closed)
                {
                    _npgsqlConnection.Close();
                }
            }
        }

        public override async Task CreateDatabaseTable(List<TableColumn> databaseolumns, string tableName, string dataBaseName, bool shouldSkipTableExistenceCheck)
        {
            try
            {
                string query = $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '{dataBaseName}' AND table_name = '{tableName}')";

                // Create a command to check table existence
                NpgsqlCommand checkExistsTableCommand = new NpgsqlCommand(query, _npgsqlConnection);

                // Execute the query and get the result
                object result = await checkExistsTableCommand.ExecuteScalarAsync();
                bool tableExists = Convert.ToBoolean(result);

                // If the table already exists and the existence check is not skipped, throw an exception
                if (tableExists && !shouldSkipTableExistenceCheck)
                    throw new TableAlreadyExistsException(tableName, dataBaseName);

                // SQL command to create the table
                string commandStringCreate = $"CREATE TABLE {tableName} (";

                // Iterate through each column and add it to the create command
                for (int i = 0; i < databaseolumns.Count; i++)
                {
                    // Replace spaces in column names with underscores
                    string colName = databaseolumns[i].Name.Replace(" ", "_");

                    // If the column name is a number, prefix it with 'col_'
                    colName = (int.TryParse(colName, out _)) ? $"col_{colName}" : colName;

                    // Define the data type for the column, handling variable-length binary data types
                    string dataType = (DataTypeHelper.IsVariableLengthBinaryDataType(databaseolumns[i].DataType)) ?
                                      $"{databaseolumns[i].DataType}({databaseolumns[i].Length})" :
                                      databaseolumns[i].DataType.Replace("_", " ");

                    // Define the constraints for the column (e.g., DEFAULT, UNIQUE)
                    string keyTable = (databaseolumns[i].ColumnOption == ColumnOption.DEFAULT) ? $"DEFAULT '{databaseolumns[i].DefaultValue}'" :
                                      (databaseolumns[i].ColumnOption == ColumnOption.UNIQUE) ? "UNIQUE" : "";

                    // Concatenate the column definition to the create command
                    commandStringCreate += $"{colName} {dataType} {keyTable} {((i != databaseolumns.Count - 1) ? "," : "")}";
                }

                commandStringCreate += ");";

                // Create a command to execute the create query
                NpgsqlCommand createSqlCommand = new NpgsqlCommand(commandStringCreate, _npgsqlConnection);

                // Execute the create query
                int success = await createSqlCommand.ExecuteNonQueryAsync();

                // If the creation is not successful, throw an exception
                if (success > 0)
                    throw new CreateTableException("Failed to create the table in the database.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override async Task ExecuteDatabaseQuery(string query)
        {
            try
            {
                using (NpgsqlCommand dbCommand = new NpgsqlCommand(query, _npgsqlConnection))
                {
                    await dbCommand.ExecuteNonQueryAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Inserts data from a DataTable into the specified database table using the provided columns and table name.
        public override async Task InsertDataIntoDatabase(DataTable dataTable, List<TableColumn> tableColumns, string tableName)
        {
            try
            {
                // Iterate through each row in the DataTable
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    // SQL command string for the data insertion
                    string commandStringInsert = $"INSERT INTO {tableName} VALUES (";

                    // Assuming _dbConnection is your IDbConnection instance
                    DbProviderFactory factory = DbProviderFactories.GetFactory(_npgsqlConnection);

                    // List to store DbParameter objects for each parameterized value
                    List<DbParameter> dbParameters = new List<DbParameter>();

                    // Get the array of items in the DataRow
                    var Items = row.ItemArray;

                    // Iterate through each item in the DataRow
                    for (int i = 0; i < Items.Length; i++)
                    {
                        // Parameter name for the parameterized query
                        string paramName = $"@param{i}";
                        commandStringInsert += $"{paramName} {((i != Items.Length - 1) ? "," : "")}";

                        // Create a parameter using the factory
                        DbParameter parameter = factory.CreateParameter();
                        parameter.ParameterName = paramName;

                        // Check if the value is a DateTime and convert it to the appropriate format
                        if (DataTypeHelper.IsDatabaseDateTimeType(tableColumns[i].DataType) &&
                            DateTime.TryParseExact(Items[i] as string, tableColumns[i].DateFormat, provider, DateTimeStyles.None, out DateTime dateTime))
                        {
                            parameter.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            parameter.Value = Items[i];
                        }

                        // Set DbType based on your data type
                        parameter.DbType = DbTypeConverter.ConvertToDbType(DataBaseType.POSTGRESQL, tableColumns[i].DataType);

                        dbParameters.Add(parameter);
                    }

                    commandStringInsert += ");";

                    // Create a command to execute the insert query
                    NpgsqlCommand sqlCommandInsert = new NpgsqlCommand(commandStringInsert, _npgsqlConnection);

                    sqlCommandInsert.Parameters.AddRange(dbParameters.ToArray());

                    await sqlCommandInsert.ExecuteNonQueryAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

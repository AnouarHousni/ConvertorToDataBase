using ConvertorToDataBase.Enums;
using ConvertorToDataBase.Exceptions;
using ConvertorToDataBase.Interfaces;
using ConvertorToDataBase.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Modules
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly DataBaseType _dataBase;
        private readonly DbConnection _dbConnection;
        private CultureInfo provider = CultureInfo.InvariantCulture;

        public DatabaseManager(DataBaseType dataBase, DbConnection dbConnection)
        {
            _dbConnection = dbConnection;

            _dataBase = dataBase;
        }

        public async Task OpenConnection() => await _dbConnection.OpenAsync();
        public async Task CloseConnection()
        {
            if (_dbConnection.State != ConnectionState.Closed)
            {
                await _dbConnection.CloseAsync();
            }
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                await _dbConnection.OpenAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // Ensure that the connection is closed, whether an exception occurred or not
                if (_dbConnection.State != ConnectionState.Closed)
                {
                    _dbConnection.Close();
                }
            }
        }

        private DbCommand CreateDatabaseCommand(string commandString)
        {
            switch (_dataBase)
            {
                case DataBaseType.MYSQL:
                    if (_dbConnection is MySqlConnection mySqlConnection)
                    {
                        return new MySqlCommand(commandString, mySqlConnection);
                    }
                    break;
                case DataBaseType.POSTGRESQL:
                    if (_dbConnection is NpgsqlConnection npgsqlConnection)
                    {
                        return new NpgsqlCommand(commandString, npgsqlConnection);
                    }
                    break;
                case DataBaseType.SQLSERVER:
                    if (_dbConnection is SqlConnection sqlConnection)
                    {
                        return new SqlCommand(commandString, sqlConnection);
                    }
                    break;
            }

            throw new ArgumentException("Unsupported database type or connection.");
        }


        public async Task CreateDatabaseTable(List<TableColumn> databaseolumns, string tableName, string dataBaseName, bool shouldSkipTableExistenceCheck)
        {
            try
            {
                string query = $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '{dataBaseName}' AND table_name = '{tableName}')";

                // Create a command to check table existence
                DbCommand checkExistsTableCommand = CreateDatabaseCommand(query);

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
                DbCommand createSqlCommand = CreateDatabaseCommand(commandStringCreate);

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

        public async Task ExecuteDatabaseQuery(string query)
        {
            try
            {
                using (DbCommand dbCommand = CreateDatabaseCommand(query))
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
        public async Task InsertDataIntoDatabase(DataTable dataTable, List<TableColumn> tableColumns, string tableName)
        {
            try
            {
                // Iterate through each row in the DataTable
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    // SQL command string for the data insertion
                    string commandStringInsert = $"INSERT INTO {tableName} VALUES (";

                    // Assuming _dbConnection is your IDbConnection instance
                    DbProviderFactory factory = DbProviderFactories.GetFactory(_dbConnection);

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
                        parameter.DbType = DbTypeConverter.ConvertToDbType(_dataBase, tableColumns[i].DataType);

                        dbParameters.Add(parameter);
                    }

                    commandStringInsert += ");";

                    // Create a command to execute the insert query
                    DbCommand sqlCommandInsert = CreateDatabaseCommand(commandStringInsert);

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

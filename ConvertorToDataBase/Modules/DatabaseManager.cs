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
                    // Handle if _dbConnection is not of the expected type
                    break;
                case DataBaseType.POSTGRESQL:
                    if (_dbConnection is NpgsqlConnection npgsqlConnection)
                    {
                        return new NpgsqlCommand(commandString, npgsqlConnection);
                    }
                    // Handle if _dbConnection is not of the expected type
                    break;
                case DataBaseType.SQLSERVER:
                    if (_dbConnection is SqlConnection sqlConnection)
                    {
                        return new SqlCommand(commandString, sqlConnection);
                    }
                    // Handle if _dbConnection is not of the expected type
                    break;
            }

            throw new ArgumentException("Unsupported database type or connection.");
        }


        public async Task CreateDatabaseTable(List<TableColumn> databaseolumns, string tableName, string dataBaseName, bool shouldSkipTableExistenceCheck)
        {
            try
            {
                string query = $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '{dataBaseName}' AND table_name = '{tableName}')";
                DbCommand checkExistsTableCommand = CreateDatabaseCommand(query);
                object result = await checkExistsTableCommand.ExecuteScalarAsync();
                bool tableExists = Convert.ToBoolean(result);

                if (tableExists && !shouldSkipTableExistenceCheck)
                    throw new TableAlreadyExistsException(tableName,dataBaseName);

                string commandStringCreate = $"create table {tableName} (";

                for (int i = 0; i < databaseolumns.Count; i++)
                {
                    string colName = databaseolumns[i].Name.Replace(" ", "_");
                    colName = (int.TryParse(colName, out _)) ? $"col_{colName}" : colName;

                    string dataType = (DataTypeHelper.IsVariableLengthBinaryDataType(databaseolumns[i].DataType)) ? $"{databaseolumns[i].DataType}({databaseolumns[i].Length})" : databaseolumns[i].DataType.Replace("_"," ");

                    string keyTable = (databaseolumns[i].ColumnOption == ColumnOption.DEFAULT) ? $"default '{databaseolumns[i].DefaultValue}'" : (databaseolumns[i].ColumnOption == ColumnOption.UNIQUE) ?
                                      "UNIQUE" : "";

                    commandStringCreate += $"{colName} {dataType} {keyTable} {((i != databaseolumns.Count - 1) ? "," : "")}";
                }

                commandStringCreate += ");";

                DbCommand createSqlCommand = CreateDatabaseCommand(commandStringCreate);
                int success = await createSqlCommand.ExecuteNonQueryAsync();

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

        public async Task InsertDataIntoDatabase(DataTable dataTable, List<TableColumn> tableColumns, string tableName)
        {
            try
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    string commandStringInsert = $"Insert into {tableName} values (";

                    // Assuming _dbConnection is your IDbConnection instance
                    DbProviderFactory factory = DbProviderFactories.GetFactory(_dbConnection);
                    List<DbParameter> dbParameters = new List<DbParameter>();
                    var Items = row.ItemArray;

                    for (int i = 0; i < Items.Length; i++)
                    {
                        string paramName = $"@param{i}";
                        commandStringInsert += $"{paramName} {((i != Items.Length - 1) ? "," : "")}";


                        // Create a parameter using the factory
                        DbParameter parameter = factory.CreateParameter();
                        parameter.ParameterName = paramName;

                        // Check if the value is a DateTime and convert it to the appropriate format
                        if (DataTypeHelper.IsDatabaseDateTimeType(tableColumns[i].DataType) && DateTime.TryParseExact(Items[i] as string, tableColumns[i].DateFormat, provider, DateTimeStyles.None, out DateTime dateTime))
                        {
                            parameter.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            // For other types, use Items[i] directly
                            parameter.Value = Items[i];
                        }

                        // Set DbType based on your data type
                        parameter.DbType = DbTypeConverter.ConvertToDbType(_dataBase, tableColumns[i].DataType);

                        dbParameters.Add(parameter);
                    }

                    commandStringInsert += ");";

                    DbCommand sqlCommandInsert = CreateDatabaseCommand(commandStringInsert);

                    // to this line
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

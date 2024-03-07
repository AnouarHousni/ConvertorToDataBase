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
    public abstract class DatabaseManager : IDatabaseManager
    {
        public abstract Task OpenConnection();
        public abstract Task CloseConnection();

        public abstract Task<bool> TestConnection();

        public abstract Task CreateDatabaseTable(List<TableColumn> databaseolumns, string tableName, string dataBaseName, bool shouldSkipTableExistenceCheck);

        public abstract Task ExecuteDatabaseQuery(string query);

        // Inserts data from a DataTable into the specified database table using the provided columns and table name.
        public abstract Task InsertDataIntoDatabase(DataTable dataTable, List<TableColumn> tableColumns, string tableName);
    }
}

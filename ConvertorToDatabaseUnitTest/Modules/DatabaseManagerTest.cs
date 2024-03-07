using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvertorToDataBase.Enums;
using ConvertorToDataBase.Exceptions;
using ConvertorToDataBase.Interfaces;
using ConvertorToDataBase.Models;
using ConvertorToDataBase.Modules;
using Moq;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Npgsql;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace ConvertorToDatabaseUnitTest.Modules
{
    public class DatabaseManagerTest
    {
        const string databaseName = "test";//
        string? connString = Environment.GetEnvironmentVariable("DbConnectionStringConvertorTest", EnvironmentVariableTarget.User);
        const DataBaseType dataBaseType = DataBaseType.MYSQL;

        private IDatabaseManager _databaseManager;
        private readonly List<(string tableName, List<TableColumn> tableColumns)> _tables;
        private readonly MySqlConnection _mySqlConnection;
        public DatabaseManagerTest()
        {
            _tables = new List<(string tableName, List<TableColumn> tableColumns)>
            {
                new ("Users" , new List<TableColumn>
                {
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "INT", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 0, Name = "ID"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "VARCHAR", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 50, Name = "Username"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "VARCHAR", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 20, Name = "Password"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "VARCHAR", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 20, Name = "Email"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "INT", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 0, Name = "ClassID"},
                }),
                new ("Class", new List<TableColumn>
                {
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "INT", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 0, Name = "ClassID"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "VARCHAR", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 50, Name = "ClassName"},
                    new TableColumn{ ColumnOption = ColumnOption.NONE, DataType = "CHAR", DateFormat = string.Empty, DefaultValue = string.Empty, Length = 1, Name = "ClassRank"},
                })
            };

            _mySqlConnection = new MySqlConnection(connString);

            _databaseManager = new MysqlDataBaseManager(_mySqlConnection);

            
        }

        private async Task DeleteAddCreate()
        {
            await _databaseManager.OpenConnection();

            await dropTables();

            for (int i = 0; i < _tables.Count; i++)
            {
                await _databaseManager.CreateDatabaseTable(_tables[i].tableColumns, _tables[i].tableName, databaseName, shouldSkipTableExistenceCheck: false);
            }
            await _databaseManager.CloseConnection();
        }

        private async Task dropTables()
        {
            if (_mySqlConnection is not MySqlConnection mySqlConnection)
            {
                throw new Exception("");
            }

            for (int i = 0; i< _tables.Count;i++)
            {
                string query = $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '{databaseName}' AND table_name = '{_tables[i].tableName}')";
                DbCommand checkExistsTableCommand = new MySqlCommand(query, mySqlConnection);
                object result = await checkExistsTableCommand.ExecuteScalarAsync();
                bool tableExists = Convert.ToBoolean(result);

                if (tableExists)
                {
                    DbCommand dropTablesCommand = new MySqlCommand($"DROP TABLE {_tables[i].tableName};", mySqlConnection);
                    await dropTablesCommand.ExecuteNonQueryAsync();
                }
            }
        }

        [Fact]
        public async Task TestConnection_Successfully()
        {
            bool canConnect = await _databaseManager.TestConnection();

            Assert.True(canConnect);
        }

        [Fact]
        public async Task TestConnection_UsingWrongConnString_ThrowException()
        {
            _databaseManager = new MysqlDataBaseManager(new MySqlConnection($"Server=127.0.0.1;uid=xxx:password=password123;database=test01;port=3306"));

            bool canConnect = await _databaseManager.TestConnection();

            Assert.False(canConnect);
        }

        [Fact]
        public async Task CreateDatabaseTable_Successfully()
        {
            await _databaseManager.OpenConnection();

            await dropTables();

            for (int i=0;i< _tables.Count;i++)
            {
                await _databaseManager.CreateDatabaseTable(_tables[i].tableColumns, _tables[i].tableName, databaseName, shouldSkipTableExistenceCheck: false);
            }

            await _databaseManager.CloseConnection();
        }

        [Fact]
        public async Task CreateDatabaseTable_UsingTableNameDoesNotExists_ThrowException()
        {
            await DeleteAddCreate();

            await _databaseManager.OpenConnection();

            for (int i = 0; i < _tables.Count; i++)
            {
                await Assert.ThrowsAsync<TableAlreadyExistsException>(async () => { await _databaseManager.CreateDatabaseTable(_tables[i].tableColumns, _tables[i].tableName, dataBaseName: databaseName, shouldSkipTableExistenceCheck: false); });
            }

            await _databaseManager.CloseConnection();
        }

        [Fact]
        public async Task ExecuteDatabaseQueryFOREIGNKEY_Successfully()
        {
            await DeleteAddCreate();

            await _databaseManager.OpenConnection();

            string tableName = _tables[0].tableName;
            string columnName = _tables[0].tableColumns[4].Name;
            string refrenceTableName = _tables[1].tableName;
            string referenceColumnName = _tables[1].tableColumns[0].Name;

            string primaryKetQurey = $"ALTER TABLE {refrenceTableName} ADD CONSTRAINT PK_C{0}_{refrenceTableName} PRIMARY KEY ({referenceColumnName})";

            string forginKeyQurey = $"ALTER TABLE {tableName} " +
                           $"ADD CONSTRAINT fk_constraint_{tableName}_{refrenceTableName} " +
                           $"FOREIGN KEY({columnName}) " +
                           $"REFERENCES {refrenceTableName}({referenceColumnName}); ";


            await _databaseManager.ExecuteDatabaseQuery(primaryKetQurey);

            await _databaseManager.ExecuteDatabaseQuery(forginKeyQurey);

            await _databaseManager.CloseConnection();
        }

        [Fact]
        public async Task ExecuteDatabaseQueryFOREIGNKEY_UsingTableDoesNotExists_ThrowException()
        {
            await DeleteAddCreate();

            await _databaseManager.OpenConnection();

            string tableName = "Student";
            string columnName = _tables[0].tableColumns[4].Name;
            string refrenceTableName = _tables[1].tableName;
            string referenceColumnName = _tables[1].tableColumns[0].Name;

            string query = $"ALTER TABLE {tableName} " +
                           $"ADD CONSTRAINT fk_constraint_{tableName}_{refrenceTableName} " +
                           $"FOREIGN KEY({columnName}) " +
                           $"REFERENCES {refrenceTableName}({referenceColumnName}); ";

            await Assert.ThrowsAsync<MySqlException>(async () => { await _databaseManager.ExecuteDatabaseQuery(query); });

            await _databaseManager.CloseConnection();
        }

        [Fact]
        public async Task InsertDataIntoDatabase_Successfully()
        {
            await DeleteAddCreate();

            await _databaseManager.OpenConnection();

            DataTable usersDataTable = new DataTable("Users");

            usersDataTable.Columns.Add("ID", typeof(int));
            usersDataTable.Columns.Add("Username", typeof(string));
            usersDataTable.Columns.Add("Password", typeof(string));
            usersDataTable.Columns.Add("Email", typeof(string));
            usersDataTable.Columns.Add("ClassID", typeof(int));

            usersDataTable.Rows.Add(1, "User1", "Password1", "user1@example.com", 1);
            usersDataTable.Rows.Add(2, "User2", "Password2", "user2@example.com", 1);
            usersDataTable.Rows.Add(3, "User3", "Password3", "user3@example.com", 2);
            usersDataTable.Rows.Add(4, "User4", "Password4", "user4@example.com", 2);
            usersDataTable.Rows.Add(5, "User5", "Password5", "user5@example.com", 2);

            DataTable classDataTable = new DataTable("Class");

            classDataTable.Columns.Add("ClassID", typeof(int));
            classDataTable.Columns.Add("ClassName", typeof(string));
            classDataTable.Columns.Add("ClassRank", typeof(char));

            classDataTable.Rows.Add(1, "ClassA", 'A');
            classDataTable.Rows.Add(2, "ClassB", 'B');
            
            await _databaseManager.InsertDataIntoDatabase(usersDataTable, _tables[0].tableColumns, _tables[0].tableName);

            await _databaseManager.InsertDataIntoDatabase(classDataTable, _tables[1].tableColumns, _tables[1].tableName);

            await _databaseManager.CloseConnection();
        }

        [Fact]
        public async Task InsertDataIntoDatabase_UsingWrongTableName_ThrowException()
        {
            await DeleteAddCreate();

            await _databaseManager.OpenConnection();

            string tableName = "Student";

            DataTable usersDataTable = new DataTable(tableName);

            usersDataTable.Columns.Add("ID", typeof(int));
            usersDataTable.Columns.Add("Username", typeof(string));
            usersDataTable.Columns.Add("Password", typeof(string));
            usersDataTable.Columns.Add("Email", typeof(string));
            usersDataTable.Columns.Add("ClassID", typeof(int));

            usersDataTable.Rows.Add(1, "User1", "Password1", "user1@example.com", 1);
            usersDataTable.Rows.Add(2, "User2", "Password2", "user2@example.com", 1);
            usersDataTable.Rows.Add(3, "User3", "Password3", "user3@example.com", 2);
            usersDataTable.Rows.Add(4, "User4", "Password4", "user4@example.com", 2);
            usersDataTable.Rows.Add(5, "User5", "Password5", "user5@example.com", 2);

            DataTable classDataTable = new DataTable("Class");

            classDataTable.Columns.Add("ClassID", typeof(int));
            classDataTable.Columns.Add("ClassName", typeof(string));
            classDataTable.Columns.Add("ClassRank", typeof(char));

            classDataTable.Rows.Add(1, "ClassA", 'A');
            classDataTable.Rows.Add(2, "ClassB", 'B');

            await Assert.ThrowsAsync<MySqlException>(async () => { await _databaseManager.InsertDataIntoDatabase(usersDataTable, _tables[0].tableColumns, tableName); });

            await _databaseManager.CloseConnection();
        }
    }
}

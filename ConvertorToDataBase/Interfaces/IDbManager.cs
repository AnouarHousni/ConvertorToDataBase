using ConvertorToDataBase.Models;
using ConvertorToDataBase.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Interfaces
{
    public interface IDatabaseManager 
    {
        Task OpenConnection();
        Task CloseConnection();
        Task<bool> TestConnection();
        Task CreateDatabaseTable(List<TableColumn> tableColumns, string tableName, string dataBaseName, bool shouldSkipTableExistenceCheck);
        Task ExecuteDatabaseQuery(string query);
        Task InsertDataIntoDatabase(DataTable dataTable, List<TableColumn> tableColumns, string tableName);
    }
}

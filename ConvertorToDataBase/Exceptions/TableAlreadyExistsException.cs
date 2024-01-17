using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Exceptions
{
    public class TableAlreadyExistsException : Exception
    {
        public TableAlreadyExistsException(string tableName, string databaseName) : base($"The table '{tableName}' already exists in the '{databaseName}' database. Do you want to continue with this table?") { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Exceptions
{
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string databaseName) :base($"Unable to establish a connection to the database '{databaseName}' with the provided credentials.")
        { }
    }
}

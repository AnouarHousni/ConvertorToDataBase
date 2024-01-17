using ConvertorToDataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Modules
{
    public static class DataTypeHelper
    {
        public static bool IsDatabaseDateTimeType(string dataType)
        {
            string upperCaseDataType = dataType.ToUpper();

            return upperCaseDataType == nameof(NpgsqlDataType.TIMESTAMPTZ) ||
                   upperCaseDataType == nameof(SqlServerDataType.DATE) ||
                   upperCaseDataType == nameof(SqlServerDataType.TIME) ||
                   upperCaseDataType == nameof(SqlServerDataType.DATETIME) ||
                   upperCaseDataType == nameof(SqlServerDataType.DATETIME2) ||
                   upperCaseDataType == nameof(SqlServerDataType.SMALLDATETIME) ||
                   upperCaseDataType == nameof(SqlServerDataType.TIMESTAMP);
        }

        public static bool IsVariableLengthBinaryDataType(string dataType)
        {
            return dataType == nameof(MysqlDataType.VARCHAR) || dataType == nameof(MysqlDataType.CHAR) ||
                   dataType == nameof(MysqlDataType.VARBINARY) || dataType == nameof(MysqlDataType.BINARY) ||
                   dataType == nameof(SqlServerDataType.BINARY) || dataType == nameof(NpgsqlDataType.BYTEA) ||
                   dataType == nameof(SqlServerDataType.NCHAR) || dataType == nameof(SqlServerDataType.NVARCHAR);
        }
    }
}

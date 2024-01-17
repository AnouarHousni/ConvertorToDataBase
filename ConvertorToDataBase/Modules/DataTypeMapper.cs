using ConvertorToDataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Modules
{
    public static class DataTypeMapper
    {

        public static string MapToDataBaseDataType(Type type , DataBaseType dataBaseType)
        {
            return (dataBaseType == DataBaseType.MYSQL)? MapToMysqlDataType(type) : (dataBaseType == DataBaseType.POSTGRESQL) ? MapToNpgsqlDataType(type) : MapToSqlServerDataType(type);
        }

        private static string MapToMysqlDataType(Type type)
        {
            if (type == typeof(int))
                return "INT";
            else if (type == typeof(short))
                return "SMALLINT";
            else if (type == typeof(long))
                return "BIGINT";
            else if (type == typeof(float))
                return "FLOAT";
            else if (type == typeof(double))
                return "DOUBLE";
            else if (type == typeof(decimal))
                return "DECIMAL";
            else if (type == typeof(DateTime))
                return "DATETIME";
            else if (type == typeof(string))
                return "VARCHAR";
            else if (type == typeof(char))
                return "CHAR";
            else if (type == typeof(bool))
                return "BIT";
            else if (type == typeof(byte[]))
                return "BINARY";
            else
                return "UNKNOWN";
        }

        private static string MapToNpgsqlDataType(Type type)
        {
            if (type == typeof(short))
                return "SMALLINT";
            else if (type == typeof(int))
                return "INTEGER";
            else if (type == typeof(long))
                return "BIGINT";
            else if (type == typeof(float))
                return "REAL";
            else if (type == typeof(double))
                return "DOUBLE_PRECISION";
            else if (type == typeof(decimal))
                return "NUMERIC";
            else if (type == typeof(DateTime))
                return "TIMESTAMP";
            else if (type == typeof(string))
                return "VARCHAR";
            else if (type == typeof(char))
                return "CHAR";
            else if (type == typeof(bool))
                return "BOOLEAN";
            else if (type == typeof(byte[]))
                return "BYTEA";
            else
                return "UNKNOWN";
        }

        private static string MapToSqlServerDataType(Type type)
        {
            if (type == typeof(int))
                return "INT";
            else if (type == typeof(short))
                return "SMALLINT";
            else if (type == typeof(long))
                return "BIGINT";
            else if (type == typeof(float))
                return "FLOAT";
            else if (type == typeof(double))
                return "REAL";
            else if (type == typeof(decimal))
                return "DECIMAL";
            else if (type == typeof(DateTime))
                return "DATETIME";
            else if (type == typeof(string))
                return "VARCHAR";
            else if (type == typeof(char))
                return "CHAR";
            else if (type == typeof(bool))
                return "BIT";
            else if (type == typeof(byte[]))
                return "VARBINARY";
            else
                return "UNKNOWN";
        }
    }
}

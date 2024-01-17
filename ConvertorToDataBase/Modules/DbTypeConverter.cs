using ConvertorToDataBase.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ConvertorToDataBase.Modules
{
    public static class DbTypeConverter
    {
        public static DbType ConvertToDbType(DataBaseType dataBase, string dataType)
        {
            string datatype = dataType.ToUpper();
            switch (dataBase)
            {
                case DataBaseType.MYSQL:
                    return ConvertToDbTypeMysql(datatype);
                case DataBaseType.SQLSERVER:
                    return ConvertToDbTypeSqlServer(datatype);
                case DataBaseType.POSTGRESQL:
                    return ConvertToDbTypeNpgsql(datatype);
                // Add cases for other databases if needed
                default:
                    throw new ArgumentException("Unsupported database type.");
            }
        }

        private static DbType ConvertToDbTypeMysql(string dataType)
        {
            if (!Enum.TryParse(dataType, out MysqlDataType mysqlDataType))
                throw new InvalidOperationException();

            switch (mysqlDataType)
            {
                case MysqlDataType.INT:
                case MysqlDataType.TINYINT:
                case MysqlDataType.SMALLINT:
                case MysqlDataType.MEDIUMINT:
                case MysqlDataType.BIGINT:
                    return DbType.Int64;
                case MysqlDataType.FLOAT:
                    return DbType.Single;
                case MysqlDataType.DOUBLE:
                    return DbType.Double;
                case MysqlDataType.DECIMAL:
                    return DbType.Decimal;
                case MysqlDataType.DATE:
                case MysqlDataType.TIME:
                case MysqlDataType.DATETIME:
                case MysqlDataType.TIMESTAMP:
                    return DbType.DateTime;
                case MysqlDataType.YEAR:
                    return DbType.Int32;
                case MysqlDataType.CHAR:
                case MysqlDataType.VARCHAR:
                case MysqlDataType.TINYTEXT:
                case MysqlDataType.TEXT:
                case MysqlDataType.MEDIUMTEXT:
                case MysqlDataType.LONGTEXT:
                    return DbType.String;
                case MysqlDataType.BINARY:
                case MysqlDataType.VARBINARY:
                    return DbType.Binary;
                case MysqlDataType.JSON:
                    return DbType.String; // Assuming JSON is stored as a string
                case MysqlDataType.ENUM:
                case MysqlDataType.SET:
                case MysqlDataType.GEOMETRY:
                    return DbType.String; // Adjust accordingly based on your specific use case
                case MysqlDataType.BIT:
                    return DbType.Boolean; // Assuming BIT is used for boolean values
                default:
                    throw new ArgumentOutOfRangeException(nameof(mysqlDataType), mysqlDataType, null);
            }
        }

        private static DbType ConvertToDbTypeSqlServer(string dataType)
        {
            if (!Enum.TryParse(dataType, out SqlServerDataType sqlServerDataType))
                throw new InvalidOperationException();

            switch (sqlServerDataType)
            {
                case SqlServerDataType.INT:
                case SqlServerDataType.SMALLINT:
                case SqlServerDataType.BIGINT:
                    return DbType.Int64;
                case SqlServerDataType.FLOAT:
                case SqlServerDataType.REAL:
                    return DbType.Single;
                case SqlServerDataType.DECIMAL:
                case SqlServerDataType.NUMERIC:
                    return DbType.Decimal;
                case SqlServerDataType.DATE:
                case SqlServerDataType.TIME:
                case SqlServerDataType.DATETIME:
                case SqlServerDataType.DATETIME2:
                case SqlServerDataType.SMALLDATETIME:
                case SqlServerDataType.TIMESTAMP:
                    return DbType.DateTime;
                case SqlServerDataType.CHAR:
                case SqlServerDataType.VARCHAR:
                case SqlServerDataType.TEXT:
                case SqlServerDataType.NCHAR:
                case SqlServerDataType.NVARCHAR:
                case SqlServerDataType.NTEXT:
                    return DbType.String;
                case SqlServerDataType.BINARY:
                case SqlServerDataType.VARBINARY:
                    return DbType.Binary;
                case SqlServerDataType.JSON:
                case SqlServerDataType.UNIQUEIDENTIFIER:
                case SqlServerDataType.XML:
                    return DbType.String; // Adjust accordingly based on your specific use case
                case SqlServerDataType.GEOMETRY:
                case SqlServerDataType.GEOGRAPHY:
                case SqlServerDataType.HIERARCHYID:
                    return DbType.Object; // Assuming these types are treated as objects
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlServerDataType), sqlServerDataType, null);
            }
        }

        private static DbType ConvertToDbTypeNpgsql(string dataType)
        {
            if (!Enum.TryParse(dataType, out NpgsqlDataType npgsqlDataType))
                throw new InvalidOperationException();

            switch (npgsqlDataType)
            {
                case NpgsqlDataType.SMALLINT:
                case NpgsqlDataType.INTEGER:
                case NpgsqlDataType.BIGINT:
                    return DbType.Int64;
                case NpgsqlDataType.NUMERIC:
                case NpgsqlDataType.REAL:
                case NpgsqlDataType.DOUBLE_PRECISION:
                    return DbType.Decimal;
                case NpgsqlDataType.DATE:
                case NpgsqlDataType.TIME:
                case NpgsqlDataType.TIMESTAMP:
                case NpgsqlDataType.TIMESTAMPTZ:
                    return DbType.DateTime;
                case NpgsqlDataType.CHAR:
                case NpgsqlDataType.VARCHAR:
                case NpgsqlDataType.TEXT:
                case NpgsqlDataType.JSON:
                case NpgsqlDataType.JSONB:
                case NpgsqlDataType.UUID:
                case NpgsqlDataType.XML:
                    return DbType.String;
                case NpgsqlDataType.BYTEA:
                    return DbType.Binary;
                case NpgsqlDataType.ARRAY:
                case NpgsqlDataType.INET:
                case NpgsqlDataType.CIDR:
                case NpgsqlDataType.MACADDR:
                case NpgsqlDataType.GEOMETRY:
                case NpgsqlDataType.GEOGRAPHY:
                    return DbType.Object; // Assuming these types are treated as objects
                default:
                    throw new ArgumentOutOfRangeException(nameof(npgsqlDataType), npgsqlDataType, null);
            }
        }
    }
}

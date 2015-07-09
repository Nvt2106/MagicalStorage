using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MySQL
{
    class MySQLDataTypeConversion : DataTypeConversion
    {
        public override string GetSQLTypeOfCSharpType(string cSharpTypeName)
        {
            switch (cSharpTypeName)
            {
                case "Int16":
                    return "smallint";
                case "Int32":
                    return "int";
                case "Int64":
                    return "bigint";
                case "Boolean":
                    return "bit";
                case "DateTime":
                    return "datetime";
                case "Guid":
                    return "varchar(36)";
                default:
                    return "varchar";
            }
        }

        public override DbType GetDbTypeOfCSharpType(string cSharpTypeName)
        {
            switch (cSharpTypeName)
            {
                case "Int16":
                    return DbType.Int16;
                case "Int32":
                    return DbType.Int32;
                case "Int64":
                    return DbType.Int64;
                case "Boolean":
                    return DbType.Boolean;
                case "DateTime":
                    return DbType.DateTime;
                case "Guid":
                    return DbType.String; // 36 chars
                default:
                    return DbType.String;
            }
        }
    }
}

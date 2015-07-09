using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MySQL
{
    class MySQLFactory : SQLFactory
    {
        internal override DataTypeConversion GetDataTypeConversion()
        {
            return new MySQLDataTypeConversion();
        }

        internal override SQLStatementBuilder GetSQLStatementBuilder(Type entityType)
        {
            return new MySQLStatementBuilder(entityType);
        }

        internal override DatabaseUpdation GetDatabaseUpdation(Database database)
        {
            return new MySQLDatabaseUpdation(database);
        }

        internal override DatabaseDeletion GetDatabaseDeletion(Database database)
        {
            return new MySQLDatabaseDeletion(database);
        }
    }
}

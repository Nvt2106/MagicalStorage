using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MSSQL
{
    class MSSQLFactory : SQLFactory
    {
        internal override DataTypeConversion GetDataTypeConversion()
        {
            return new MSSQLDataTypeConversion();
        }

        internal override SQLStatementBuilder GetSQLStatementBuilder(Type entityType)
        {
            return new MSSQLStatementBuilder(entityType);
        }

        internal override DatabaseUpdation GetDatabaseUpdation(Database database)
        {
            return new MSSQLDatabaseUpdation(database);
        }

        internal override DatabaseDeletion GetDatabaseDeletion(Database database)
        {
            return new MSSQLDatabaseDeletion(database);
        }
    }
}

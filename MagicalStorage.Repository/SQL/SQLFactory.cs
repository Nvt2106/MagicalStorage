using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    abstract class SQLFactory
    {
        internal abstract DataTypeConversion GetDataTypeConversion();

        internal abstract SQLStatementBuilder GetSQLStatementBuilder(Type entityType);

        internal DatabaseCreator GetDatabaseCreator(Type entityType, Database database)
        {
            var sqlStatementBuilder = GetSQLStatementBuilder(entityType);
            return new DatabaseCreator(sqlStatementBuilder, database);
        }

        internal abstract DatabaseUpdation GetDatabaseUpdation(Database database);

        internal abstract DatabaseDeletion GetDatabaseDeletion(Database database);
    }
}

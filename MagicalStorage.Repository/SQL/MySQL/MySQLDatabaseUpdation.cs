using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MySQL
{
    class MySQLDatabaseUpdation : DatabaseUpdation
    {
        public MySQLDatabaseUpdation(Database database)
            : base(database)
        {
            sqlFactory = new MySQLFactory();
        }

        internal override string BuildSQLStringToExecuteUpdationStoredProc(Type entityType)
        {
            var sqlStatementBuilder = sqlFactory.GetSQLStatementBuilder(entityType);

            var sbSQL = new StringBuilder("{ ");
            sbSQL.AppendFormat("CALL {0} (", sqlStatementBuilder.UpdationStoredProcName);

            var listSingularStoreProperties = sqlStatementBuilder.GetAllSingularStorePropertiesOfEntityType();
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                sbSQL.Append("?,");
            }
            sbSQL.Remove(sbSQL.Length - 1, 1);
            sbSQL.Append(") }");

            return sbSQL.ToString();
        }
    }
}

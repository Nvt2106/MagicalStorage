using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MSSQL
{
    class MSSQLDatabaseUpdation : DatabaseUpdation
    {
        public MSSQLDatabaseUpdation(Database database)
            : base(database)
        {
            sqlFactory = new MSSQLFactory();
        }
        
        internal override string BuildSQLStringToExecuteUpdationStoredProc(Type entityType)
        {
            var sqlStatementBuilder = sqlFactory.GetSQLStatementBuilder(entityType);
            return "dbo." + sqlStatementBuilder.UpdationStoredProcName;
        }
    }
}

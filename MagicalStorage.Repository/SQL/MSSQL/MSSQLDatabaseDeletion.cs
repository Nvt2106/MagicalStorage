using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MSSQL
{
    class MSSQLDatabaseDeletion : DatabaseDeletion
    {
        public MSSQLDatabaseDeletion(Database database)
            : base(database)
        {
            sqlFactory = new MSSQLFactory();
        }

        internal override string BuildSQLStringToExecuteDeletionStoredProc(string storedProcName)
        {
            return "dbo." + storedProcName;
        }
    }
}

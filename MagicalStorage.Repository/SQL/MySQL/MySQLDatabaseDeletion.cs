using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MySQL
{
    class MySQLDatabaseDeletion : DatabaseDeletion
    {
        public MySQLDatabaseDeletion(Database database)
            : base(database)
        {
            sqlFactory = new MySQLFactory();
        }

        internal override string BuildSQLStringToExecuteDeletionStoredProc(string storedProcName)
        {
            return "{ " + String.Format("CALL {0} (?)", storedProcName) + " }";
        }
    }
}

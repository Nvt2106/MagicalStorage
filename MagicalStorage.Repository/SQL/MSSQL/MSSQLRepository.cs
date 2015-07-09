using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MSSQL
{
    /// <summary>
    /// This class provides repository functions to communicate with MS SQL database.
    /// All functions in this class is called automatically by framework.
    /// Developer should NOT call any function directly.
    /// </summary>
    public class MSSQLRepository : SQLRepository
    {
        public MSSQLRepository()
            : base()
        {
            sqlFactory = new MSSQLFactory();
        }
    }
}

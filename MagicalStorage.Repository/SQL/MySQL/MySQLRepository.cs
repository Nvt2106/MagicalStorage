using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MySQL
{
    /// <summary>
    /// This class provides repository functions to communicate with MySQL database.
    /// All functions in this class is called automatically by framework.
    /// Developer should NOT call any function directly.
    /// </summary>
    public class MySQLRepository : SQLRepository
    {
        public MySQLRepository()
            : base()
        {
            sqlFactory = new MySQLFactory();
        }
    }
}

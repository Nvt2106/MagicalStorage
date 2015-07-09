using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    /// <summary>
    /// This class provides functions to create database objects for MSSQL.
    /// </summary>
    public class MSSQLDatabaseCreator : DatabaseCreator
    {
        /// <summary>
        /// Constructor to instantiate this object.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <remarks>
        /// Param entityType must be not null; otherwise, exception is thrown.
        /// </remarks>
        public MSSQLDatabaseCreator(Type entityType, Database database, ISQLStatementBuilder sqlStatementBuilder)
            : base(entityType, database, sqlStatementBuilder)
        {
        }

        /// <summary>
        /// Check if database table for the entity type exists.
        /// </summary>
        /// <returns>Boolean</returns>
        /// <remarks>It is virtual to support unit testing</remarks>
        public override bool IsDatabaseTableExisted()
        {
            return true;
        }

        /// <summary>
        /// Create a new database table for entity type (assume it doesn't exist before).
        /// </summary>
        public override void CreateDatabaseTable()
        {
            // TODO
        }
    }
}

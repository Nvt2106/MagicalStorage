using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    abstract class DatabaseUpdation
    {
        // Store EntLib Database instance to communicate with database server
        public Database Database { get; private set; }

        // Store SQLFactory instance
        internal SQLFactory sqlFactory;

        /// <summary>
        /// Constructor to instantiate this object.
        /// </summary>
        /// <param name="database">Database</param>
        public DatabaseUpdation(Database database)
        {
            this.Database = database;
        }

        /// <summary>
        /// SQL string to execute updation stored proc
        /// </summary>
        internal abstract string BuildSQLStringToExecuteUpdationStoredProc(Type entityType);

        public void Update(object entityData)
        {
            var entityType = entityData.GetType();
            var cmd = this.Database.GetStoredProcCommand(BuildSQLStringToExecuteUpdationStoredProc(entityType));

            var sqlStatementBuilder = sqlFactory.GetSQLStatementBuilder(entityType);
            var listPIs = sqlStatementBuilder.GetAllSingularStorePropertiesOfEntityType();
            var dataTypeConversion = sqlFactory.GetDataTypeConversion();
            foreach (var pi in listPIs)
            {
                var dbType = dataTypeConversion.GetDbTypeOfCSharpType(pi.PropertyType.Name);
                object valueObj = pi.GetValue(entityData, null);
                object correctValueObj = valueObj;
                if (dbType == DbType.String)
                {
                    string valueStr = String.Empty;
                    if (valueObj != null)
                    {
                        valueStr = valueObj.ToString();
                    }
                    // If string contains ' => error, so replace ' with ''
                    valueStr = valueStr.Replace("'", "''");
                    correctValueObj = valueStr;
                }
                this.Database.AddInParameter(cmd, pi.Name + "Param", dbType, correctValueObj);
            }
            this.Database.ExecuteNonQuery(cmd);
        }
    }
}

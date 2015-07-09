using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    abstract class DatabaseDeletion
    {
        // Store SQLFactory instance 
        internal SQLFactory sqlFactory;

        // Store EntLib Database instance to communicate with database server
        public Database Database {get; private set; }

        public DatabaseDeletion(Database database)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(database, "database");
            
            this.Database = database;
        }

        public void Delete(object entityData)
        {
            var entityType = entityData.GetType();
            var sqlStatementBuilder = sqlFactory.GetSQLStatementBuilder(entityType);
            var storedProcName = sqlStatementBuilder.DeletionStoredProcName;
            var cmd = this.Database.GetStoredProcCommand(BuildSQLStringToExecuteDeletionStoredProc(storedProcName));

            var dataTypeConversion = sqlFactory.GetDataTypeConversion();
            var entityId = ((IMSEntity)entityData).EntityId;
            var dbType = dataTypeConversion.GetDbTypeOfCSharpType("Guid");
            if (dbType == DbType.Guid)
            {
                this.Database.AddInParameter(cmd, MSConstants.NameOfPrimaryIdProperty + "Param", dbType, entityId);
            }
            else
            {
                this.Database.AddInParameter(cmd, MSConstants.NameOfPrimaryIdProperty + "Param", dbType, entityId.ToString());
            }
            
            this.Database.ExecuteNonQuery(cmd);
        }

        internal abstract string BuildSQLStringToExecuteDeletionStoredProc(string storedProcName);
    }
}

using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    public abstract class SQLRepository : IMSRepository
    {
        // Store EntLib Database instance to communicate with database server
        internal Database database;

        // Store factory to create family of classes
        internal SQLFactory sqlFactory;

        public SQLRepository()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());
            database = DatabaseFactory.CreateDatabase();
        }

        /// <summary>
        /// Prepare database table and all neccessary stored procedures for given entity.
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <remarks>
        /// Param entityType must be not null; otherwise, exception is thrown.
        /// Type entityType must implement IMSEntity interface; otherwise, exception is thrown.
        /// This method throws exception if any failure.
        /// </remarks>
        public void PreparePersistStorageForEntityType(Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Param entityType must implement IMSEntity interface");
            }

            var databaseCreator = sqlFactory.GetDatabaseCreator(entityType, database);
            databaseCreator.CheckExistanceThenCreateDatabaseTable();
            databaseCreator.CheckExistanceThenCreateUpdationStoredProcedure();
            databaseCreator.CheckExistanceThenCreateDeletionStoredProcedure();
        }

        public ArrayList FetchDataFromPersistStorageForEntityType(Type entityType, MSConditions conditions, MSPageSetting pageSetting)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Param entityType must implement IMSEntity interface");
            }

            var sqlStatementBuilder = sqlFactory.GetSQLStatementBuilder(entityType);
            string sqlString = sqlStatementBuilder.BuildSQLStringToSelectEntity(conditions, pageSetting);
            var dataSet = PopulateParametersAndExecute(entityType, conditions, sqlString);

            return CreateEntityListFromDataSet(entityType, dataSet);
        }

        private DataSet PopulateParametersAndExecute(Type entityType, MSConditions conditions, string sSQL)
        {
            var cmd = database.GetSqlStringCommand(sSQL);
            var allConditions = AllSingleSearchConditions(conditions);
            var dataTypeConversion = sqlFactory.GetDataTypeConversion();
            for (int i = 0; i < allConditions.Count; i++)
            {
                var condition = (MSCondition)allConditions[i];
                var pi = entityType.GetProperty(condition.PropertyName);
                string dbColumnName = condition.PropertyName;
                var dbType = dataTypeConversion.GetDbTypeOfCSharpType(pi.PropertyType.Name);
                if (dbType == DbType.String)
                {
                    // To fix MySQL error: Failed to convert parameter value from a Guid to a String.
                    string valueStr = String.Empty;
                    if (condition.PropertyValue != null)
                    {
                        // If string contains ' => error, so replace ' with ''
                        valueStr = condition.PropertyValue.ToString().Replace("'", "''");
                    }
                    database.AddInParameter(cmd, "@Var" + (i + 1).ToString(), dbType, valueStr);
                }
                else
                {
                    database.AddInParameter(cmd, "@Var" + (i + 1).ToString(), dbType, condition.PropertyValue);
                }
            }

            return database.ExecuteDataSet(cmd);
        }

        private ArrayList AllSingleSearchConditions(MSConditions conditions)
        {
            var result = new ArrayList();
            if (conditions != null)
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    object condition = conditions[i];
                    if (condition is MSCondition)
                    {
                        result.Add(condition);
                    }
                    else
                    {
                        // De quy
                        result.AddRange(AllSingleSearchConditions((MSConditions)condition));
                    }
                }
            }
            return result;
        }

        private ArrayList CreateEntityListFromDataSet(Type entityType, DataSet ds)
        {
            if ((ds == null) || (ds.Tables.Count != 1) || (ds.Tables[0].Rows.Count == 0))
            {
                return null;
            }

            var result = new ArrayList();
            foreach (DataRow dataRow in ds.Tables[0].Rows)
            {
                object item = CreateEntityInstanceFromDataRow(entityType, dataRow);
                result.Add(item);
            }

            return result;
        }

        private object CreateEntityInstanceFromDataRow(Type entityType, DataRow dataRow)
        {
            var entity = Activator.CreateInstance(entityType);

            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                var pi = entityType.GetProperty(dataColumn.ColumnName);
                
                // If no property in object matches with column in DB, then just ignore
                if (pi == null) continue;

                object valueObj = null;
                if (dataRow[dataColumn] != DBNull.Value)
                {
                    if ("Guid".Equals(pi.PropertyType.Name))
                    {
                        // MySQL doesn't have Guid, but store it as string(36)
                        valueObj = Guid.Parse(dataRow[dataColumn].ToString());
                    }
                    else
                    {
                        valueObj = dataRow[dataColumn];
                    }
                }
                pi.SetValue(entity, valueObj, null);
            }

            return entity;
        }

        /// <summary>
        /// Save entity data to database.
        /// </summary>
        /// <param name="entityData">Entity data to be saved</param>
        /// <remarks>
        /// Param entityData must be not null; otherwise, exception is thrown.
        /// This method throws exception if any failure.
        /// </remarks>
        public object SaveEntityDataToPersistStorage(object entityData)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityData, "entityData");
            var entityType = entityData.GetType();
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Type of param entityData must implement IMSEntity interface");
            }

            var databaseUpdation = sqlFactory.GetDatabaseUpdation(database);
            databaseUpdation.Update(entityData);

            return entityData;
        }

        /// <summary>
        /// Delete entity data from database.
        /// </summary>
        /// <param name="entityData">Entity data to be deleted</param>
        /// <remarks>
        /// Param entityData must be not null; otherwise, exception is thrown.
        /// This method throws exception if any failure.
        /// </remarks>
        public void DeleteEntityFromPersistStorage(object entityData)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityData, "entityData");
            var entityType = entityData.GetType();
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Type of param entityData must implement IMSEntity interface");
            }

            var databaseDeletion = sqlFactory.GetDatabaseDeletion(database);
            databaseDeletion.Delete(entityData);
        }
    }
}

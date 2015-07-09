using MagicalStorage.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
namespace MagicalStorage.Repository
{
    /// <summary>
    /// This base class provides functions to build SQL statement for various kind of purposes.
    /// </summary>
    abstract class SQLStatementBuilder
    {
        // Store entity type which is reference to database object
        public Type EntityType { get; private set; }

        protected DataTypeConversion dataTypeConversion;

        public SQLStatementBuilder(Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Param entityType must implement IMSEntity interface");
            }
            
            this.EntityType = entityType;
        }

        public string DatabaseTableName
        {
            get { return this.EntityType.Name; }
        }

        // Store sql string to check database table existance
        private string sqlStringToCheckDatabaseTableExistance;

        /// <summary>
        /// SQL string to check database table existance.
        /// </summary>
        public string SQLStringToCheckDatabaseTableExistance
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCheckDatabaseTableExistance))
                {
                    sqlStringToCheckDatabaseTableExistance =
                        BuildSQLStringToCheckDatabaseTableExistance();
                }

                return sqlStringToCheckDatabaseTableExistance;
            }
        }

        internal abstract string BuildSQLStringToCheckDatabaseTableExistance();

        // Store sql string to create database table
        private string sqlStringToCreateDatabaseTable;

        /// <summary>
        /// SQL string to create new database table.
        /// </summary>        
        public string SQLStringToCreateDatabaseTable
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCreateDatabaseTable))
                {
                    sqlStringToCreateDatabaseTable = BuildSQLStringToCreateDatabaseTable();
                }

                return sqlStringToCreateDatabaseTable;
            }
        }

        internal abstract string BuildSQLStringToCreateDatabaseTable();

        // Store sql string to check existance of update store proc
        private string sqlStringToCheckUpdationStoredProcExistance;

        /// <summary>
        /// SQL string to check existance of store proc to update entity data.
        /// </summary>
        public string SQLStringToCheckUpdationStoredProcExistance
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCheckUpdationStoredProcExistance))
                {
                    sqlStringToCheckUpdationStoredProcExistance = BuildSQLStringToCheckUpdationStoredProcExistance();
                }
                return sqlStringToCheckUpdationStoredProcExistance;
            }
        }

        internal abstract string BuildSQLStringToCheckUpdationStoredProcExistance();

        public string UpdationStoredProcName
        {
            get
            {
                return String.Format("sp{0}_Update", this.EntityType.Name);
            }            
        }

        // Store sql string to check existance of delete store proc
        private string sqlStringToCheckDeletionStoredProcExistance;

        /// <summary>
        /// SQL string to check existance of store proc to delete entity.
        /// </summary>
        public string SQLStringToCheckDeletionStoredProcExistance
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCheckDeletionStoredProcExistance))
                {
                    sqlStringToCheckDeletionStoredProcExistance = BuildSQLStringToCheckDeletionStoredProcExistance();
                }
                return sqlStringToCheckDeletionStoredProcExistance;
            }
        }

        internal abstract string BuildSQLStringToCheckDeletionStoredProcExistance();

        public string DeletionStoredProcName
        {
            get
            {
                return String.Format("sp{0}_Delete", this.EntityType.Name);
            }            
        }

        // Store sql string to create stored proc to update entity data
        private string sqlStringToCreateUpdationStoredProc;

        /// <summary>
        /// SQL string to create stored proc to update entity data.
        /// </summary>
        public string SQLStringToCreateUpdationStoredProc
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCreateUpdationStoredProc))
                {
                    sqlStringToCreateUpdationStoredProc = BuildSQLStringToCreateUpdationStoredProc();
                }
                return sqlStringToCreateUpdationStoredProc;
            }
        }

        internal abstract string BuildSQLStringToCreateUpdationStoredProc();

        // Store sql string to create stored proc to delete entity
        private string sqlStringToCreateDeletionStoredProc;

        /// <summary>
        /// SQL string to create stored proc to delete entity.
        /// </summary>
        public string SQLStringToCreateDeletionStoredProc
        {
            get
            {
                if (String.IsNullOrEmpty(sqlStringToCreateDeletionStoredProc))
                {
                    sqlStringToCreateDeletionStoredProc = BuildSQLStringToCreateDeletionStoredProc();
                }
                return sqlStringToCreateDeletionStoredProc;
            }
        }

        internal abstract string BuildSQLStringToCreateDeletionStoredProc();


        public string BuildSQLStringToSelectEntity(MSConditions conditions, MSPageSetting pageSetting)
        {
            string whereSQL = BuildWhereClause(conditions);
            string orderBySQL = BuildOrderByClause(pageSetting);

            int pageSize = 0;
            int pageNumber = 1;
            if (pageSetting != null)
            {
                pageSize = pageSetting.PageSize;
                pageNumber = pageSetting.PageIndex;
            }

            return BuildSQLStringToSelectEntityWithPaging(whereSQL, orderBySQL, pageSize, pageNumber);
        }

        private string BuildWhereClause(MSConditions conditions)
        {
            var whereSQL = new StringBuilder("IsDeleted = 0");
            string temp = BuildWhereClauseForAllConditions(conditions, 1);
            if (!String.IsNullOrWhiteSpace(temp))
            {
                whereSQL.Append(" AND ");
                whereSQL.Append(temp);
            }
            return whereSQL.ToString();
        }

        private string BuildWhereClauseForAllConditions(MSConditions conditions, int conditionIndex)
        {
            var result = new StringBuilder();
            if ((conditions != null) && (conditions.Count > 0))
            {
                result.Append("(");

                // The first condition
                object condition = conditions[0];
                if (condition is MSCondition)
                {
                    result.Append(BuildWhereClauseForSingleCondition((MSCondition)condition, conditionIndex));
                }
                else
                {
                    // De quy
                    result.Append(BuildWhereClauseForAllConditions((MSConditions)condition, conditionIndex));
                }

                string strCombine = (conditions.IsAND) ? " AND " : " OR ";
                for (var i = 1; i < conditions.Count; i++)
                {
                    result.Append(strCombine);

                    condition = conditions[i];
                    if (condition is MSCondition)
                    {
                        result.Append(BuildWhereClauseForSingleCondition((MSCondition)condition, conditionIndex + i));
                    }
                    else
                    {
                        // De quy
                        result.Append(BuildWhereClauseForAllConditions((MSConditions)condition, conditionIndex + i));
                    }
                }

                result.Append(")");
            }
            return result.ToString();
        }

        internal abstract string BuildWhereClauseForSingleCondition(MSCondition condition, int conditionIndex);

        private string BuildOrderByClause(MSPageSetting pageSetting)
        {
            var sortSQL = new StringBuilder("IsDeleted");
            if (pageSetting != null)
            {
                // Do not use foreach sorting needs to be in order
                for (var i = 0; i < pageSetting.SortInfos.Count; i++)
                {
                    var sortInfo = pageSetting.SortInfos[i];
                    sortSQL.Append(", ");
                    sortSQL.Append((sortInfo.SortDirection == MSSortDirection.Asc) ? sortInfo.PropertyName : sortInfo.PropertyName + " DESC");
                }
            }
            return sortSQL.ToString();
        }

        internal abstract string BuildSQLStringToSelectEntityWithPaging(string whereSQL, string orderBySQL, int pageSize, int pageNumber);

        internal abstract List<PropertyInfo> GetAllSingularStorePropertiesOfEntityType();
    }
}

using MagicalStorage.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.MSSQL
{
    /// <summary>
    /// This class provides functions to build SQL statement for various kind of purposes.
    /// </summary>
    class MSSQLStatementBuilder : SQLStatementBuilder
    {
        /// <summary>
        /// Constructor to instantiate object of this class.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <remarks>
        /// Param entityType must be not null; otherwise, exception is thrown
        /// Type entityType must implement IMSEntity interface; otherwise, exception is thrown
        /// </remarks>
        public MSSQLStatementBuilder(Type entityType)
            : base(entityType)
        {
            dataTypeConversion = new MSSQLDataTypeConversion();
        }

        /// <summary>
        /// SQL string to check database table existance.
        /// </summary>
        internal override string BuildSQLStringToCheckDatabaseTableExistance()
        {
            return String.Format("SELECT COUNT(*) FROM sysobjects WHERE xtype='U' AND [Name] = '{0}'", this.DatabaseTableName);
        }

        /// <summary>
        /// SQL string to create new database table.
        /// </summary>
        /// <remarks>
        /// Example:
        /// CREATE TABLE [dbo].[AppConfig]
        /// (
        /// 	[EntityId] [uniqueidentifier] NOT NULL,
        /// 	[AppId] [uniqueidentifier] NOT NULL,
        /// 	[IosP12] [nvarchar](255) NULL,
        /// 	[IosP12Password] [nvarchar](255) NULL,
        /// 	[IsDeleted] [bit] NOT NULL
        /// )
        /// ON [PRIMARY]
        /// 
        /// [IsDeleted] is added column to logically delete entity.
        /// </remarks>
        internal override string BuildSQLStringToCreateDatabaseTable()
        {
            var sbSQL = new StringBuilder();
            sbSQL.Append(String.Format("CREATE TABLE [dbo].[{0}]", this.DatabaseTableName)).Append(Environment.NewLine);
            sbSQL.Append("(").Append(Environment.NewLine);

            var listSingularStoreProperties = GetAllSingularStorePropertiesOfEntityType();
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                string oneColumn = SQLStringToCreateDatabaseTable_OneColumn(propertyInfo);
                sbSQL.Append(oneColumn).Append(",");
            }
            sbSQL.Append("[IsDeleted] bit NOT NULL DEFAULT(0))").Append(Environment.NewLine);
            sbSQL.Append("ON [PRIMARY];").Append(Environment.NewLine);

            return sbSQL.ToString();
        }

        private string SQLStringToCreateDatabaseTable_OneColumn(PropertyInfo propertyInfo)
        {
            string columnType = dataTypeConversion.GetSQLTypeOfCSharpType(propertyInfo.PropertyType.Name);
            string columnSize = String.Empty;
            if (MSTypeHelper.IsStringType(propertyInfo.PropertyType))
            {
                int maxLength = MSPropertyHelper.GetStringMaxLength(propertyInfo);
                if (maxLength > 0)
                {
                    columnSize = String.Format("({0})", maxLength);
                }
            }
            var mandatory = MSPropertyHelper.IsMandatoryProperty(propertyInfo) ? "NOT NULL" : "NULL";
            return String.Format("[{0}] {1}{2} {3}", propertyInfo.Name, columnType, columnSize, mandatory);
        }

        private const string SQL_CHECK_SP_EXISTS = "SELECT COUNT(*) FROM sysobjects WHERE xtype = 'p' AND [name] = '{0}'";

        /// <summary>
        /// SQL string to check existance of store proc to update entity data.
        /// </summary>
        internal override string BuildSQLStringToCheckUpdationStoredProcExistance()
        {
            return String.Format(SQL_CHECK_SP_EXISTS, this.UpdationStoredProcName);
        }

        /// <summary>
        /// SQL string to check existance of store proc to delete entity.
        /// </summary>
        internal override string BuildSQLStringToCheckDeletionStoredProcExistance()
        {
            return String.Format(SQL_CHECK_SP_EXISTS, this.DeletionStoredProcName);
        }

        /// <summary>
        /// SQL string to create stored proc to update entity data.
        /// </summary>
        internal override string BuildSQLStringToCreateUpdationStoredProc()
        {
            var listSingularStoreProperties = GetAllSingularStorePropertiesOfEntityType();

            var sbSQL = new StringBuilder();
            // ---------------Header-------------------
            sbSQL.Append(String.Format("CREATE PROCEDURE dbo.{0}", this.UpdationStoredProcName)).Append(Environment.NewLine);
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                string oneParam = SQLStringToCreateUpdationStoredProc_OneParam(propertyInfo);
                sbSQL.Append(oneParam).Append(",");
            }
            if (sbSQL.ToString().EndsWith(","))
            {
                sbSQL.Remove(sbSQL.Length - 1, 1);
            }
            sbSQL.Append(" AS BEGIN").Append(Environment.NewLine);
            // ----------------Body------------------
            sbSQL.Append("SET NOCOUNT ON;").Append(Environment.NewLine);
            sbSQL.Append(String.Format("IF EXISTS(SELECT * FROM dbo.[{0}] WHERE {1} = @{2}Param)", this.DatabaseTableName, MSConstants.NameOfPrimaryIdProperty, MSConstants.NameOfPrimaryIdProperty)).Append(Environment.NewLine);
            sbSQL.Append("BEGIN").Append(Environment.NewLine);
            sbSQL.Append(String.Format("UPDATE dbo.[{0}]", this.DatabaseTableName)).Append(Environment.NewLine);
            sbSQL.Append("SET ").Append(Environment.NewLine);
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                sbSQL.Append(String.Format("{0} = @{1}Param,", propertyInfo.Name, propertyInfo.Name));
            }
            sbSQL.Remove(sbSQL.Length - 1, 1).Append(Environment.NewLine);

            sbSQL.Append(String.Format("WHERE {0} = @{1}Param", MSConstants.NameOfPrimaryIdProperty, MSConstants.NameOfPrimaryIdProperty)).Append(Environment.NewLine);
            sbSQL.Append("END").Append(Environment.NewLine);
            // ----------------------------------
            sbSQL.Append("ELSE BEGIN").Append(Environment.NewLine);
            sbSQL.Append(String.Format("INSERT dbo.[{0}](", this.DatabaseTableName));
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                sbSQL.Append(propertyInfo.Name + ",");
            }
            if (sbSQL.ToString().EndsWith(","))
            {
                sbSQL.Remove(sbSQL.Length - 1, 1);
            }
            sbSQL.Append(")").Append(Environment.NewLine);
            sbSQL.Append("VALUES(");
            foreach (var propertyInfo in listSingularStoreProperties)
            {
                sbSQL.Append("@" + propertyInfo.Name + "Param,");
            }
            if (sbSQL.ToString().EndsWith(","))
            {
                sbSQL.Remove(sbSQL.Length - 1, 1);
            }
            sbSQL.Append(")").Append(Environment.NewLine);
            sbSQL.Append("END").Append(Environment.NewLine);
            // ----------------------------------
            sbSQL.Append("END");

            return sbSQL.ToString();
        }

        private string SQLStringToCreateUpdationStoredProc_OneParam(PropertyInfo propertyInfo)
        {
            string paramType = dataTypeConversion.GetSQLTypeOfCSharpType(propertyInfo.PropertyType.Name);
            string stringSize = String.Empty;
            if (MSTypeHelper.IsStringType(propertyInfo.PropertyType))
            {
                int maxLength = MSPropertyHelper.GetStringMaxLength(propertyInfo);
                if (maxLength > 0)
                {
                    stringSize = String.Format("({0})", maxLength);
                }
            }
            return String.Format("@{0}Param {1}{2}", propertyInfo.Name, paramType, stringSize);
        }

        /// <summary>
        /// SQL string to create stored proc to delete entity.
        /// </summary>
        internal override string BuildSQLStringToCreateDeletionStoredProc()
        {
            return String.Format("CREATE PROCEDURE dbo.{0} @{1}Param uniqueidentifier AS BEGIN SET NOCOUNT ON; UPDATE dbo.[{2}] SET IsDeleted = 1 WHERE {3} = @{4}Param AND IsDeleted = 0; END",
                this.DeletionStoredProcName, MSConstants.NameOfPrimaryIdProperty, this.DatabaseTableName, MSConstants.NameOfPrimaryIdProperty, MSConstants.NameOfPrimaryIdProperty);
        }

        internal override string BuildWhereClauseForSingleCondition(MSCondition condition, int conditionIndex)
        {
            if (condition.IsSingleOperatorCondition())
            {
                return String.Format("([{0}] {1})", condition.PropertyName, dataTypeConversion.ConvertCompareOperatorEnumToSQLString(condition.CompareOperator));
            }
            else
            {
                return String.Format("([{0}] {1} @Var{2})", condition.PropertyName, dataTypeConversion.ConvertCompareOperatorEnumToSQLString(condition.CompareOperator), conditionIndex);
            }
        }

        internal override string BuildSQLStringToSelectEntityWithPaging(string whereSQL, string orderBySQL, int pageSize, int pageNumber)
        {
            string sqlString;
            if (pageSize > 0)
            {
                int endIndex = pageNumber * pageSize;
                int startIndex = endIndex - pageSize + 1;
                sqlString = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY " + orderBySQL + ")  RowNr, * FROM dbo.[" + this.DatabaseTableName + "] WHERE " + whereSQL.ToString() + ") t WHERE RowNr BETWEEN " + startIndex.ToString() + " AND " + endIndex.ToString();
            }
            else
            {
                sqlString = "SELECT * FROM dbo.[" + this.DatabaseTableName + "] WHERE " + whereSQL.ToString() + " ORDER BY " + orderBySQL;
            }
            return sqlString;
        }

        internal override List<PropertyInfo> GetAllSingularStorePropertiesOfEntityType()
        {
            return MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(this.EntityType);
        }
    }
}

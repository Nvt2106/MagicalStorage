using MagicalStorage.Core;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    /// <summary>
    /// This class provides functions to create database objects (table, stored procedure, etc).
    /// </summary>
    class DatabaseCreator
    {
        // Store SQLStatementBuilder instance 
        public SQLStatementBuilder SQLStatementBuilder { get; private set; }

        // Store EntLib Database instance to communicate with database server
        public Database Database {get; private set; }

        /// <summary>
        /// Constructor to instantiate this object.
        /// </summary>
        /// <param name="sqlStatementBuilder">SQLStatementBuilder instance</param>
        /// <param name="database">Database instance</param>
        /// <remarks>
        /// All params must be not null; otherwise, exception is thrown.
        /// </remarks>
        public DatabaseCreator(SQLStatementBuilder sqlStatementBuilder, Database database)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(sqlStatementBuilder, "sqlStatementBuilder");
            MSParameterHelper.MakeSureInputParameterNotNull(database, "database");
            
            this.SQLStatementBuilder = sqlStatementBuilder;
            this.Database = database;
        }

        /// <summary>
        /// Check if database table for the entity type exists.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsDatabaseTableExisted()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCheckDatabaseTableExistance;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                var ds = this.Database.ExecuteDataSet(cmd);
                return (!"0".Equals(ds.Tables[0].Rows[0][0].ToString()));
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot check database table existance", exp);
            }
        }

        /// <summary>
        /// Create a new database table for entity type (assume it doesn't exist before).
        /// </summary>
        public void CreateDatabaseTable()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCreateDatabaseTable;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                this.Database.ExecuteNonQuery(cmd);
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot create database table", exp);
            }
        }

        /// <summary>
        /// Check database table existance, if not, create a new one.
        /// </summary>
        public void CheckExistanceThenCreateDatabaseTable()
        {
            if (!this.IsDatabaseTableExisted())
            {
                CreateDatabaseTable();
            }
        }

        /// <summary>
        /// Check if the updation stored proc exists for this entity type.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsUpdationStoredProcedureExisted()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCheckUpdationStoredProcExistance;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                var ds = this.Database.ExecuteDataSet(cmd);
                return (!"0".Equals(ds.Tables[0].Rows[0][0].ToString()));
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot check updation stored proc existance", exp);
            }
        }

        /// <summary>
        /// Create stored proc to update entity data (assume it doesn't exist yet)
        /// </summary>
        public void CreateUpdationStoredProcedure()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCreateUpdationStoredProc;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                this.Database.ExecuteNonQuery(cmd);
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot create updation stored proc", exp);
            }
        }

        /// <summary>
        /// Check updation stored proc existance, if not, create a new one.
        /// </summary>
        public void CheckExistanceThenCreateUpdationStoredProcedure()
        {
            if (!this.IsUpdationStoredProcedureExisted())
            {
                CreateUpdationStoredProcedure();
            }
        }

        /// <summary>
        /// Check if the deletion stored proc exists for this entity type.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsDeletionStoredProcedureExisted()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCheckDeletionStoredProcExistance;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                var ds = this.Database.ExecuteDataSet(cmd);
                return (!"0".Equals(ds.Tables[0].Rows[0][0].ToString()));
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot check deletion stored proc existance", exp);
            }
        }

        /// <summary>
        /// Create stored proc to delete entity (assume it doesn't exist yet)
        /// </summary>
        public void CreateDeletionStoredProcedure()
        {
            var sqlString = this.SQLStatementBuilder.SQLStringToCreateDeletionStoredProc;
            try
            {
                var cmd = this.Database.GetSqlStringCommand(sqlString);
                this.Database.ExecuteNonQuery(cmd);
            }
            catch (Exception exp)
            {
                throw new RepositoryException("Cannot create updation stored proc", exp);
            }
        }

        /// <summary>
        /// Check deletion stored proc existance, if not, create a new one.
        /// </summary>
        public void CheckExistanceThenCreateDeletionStoredProcedure()
        {
            if (!this.IsDeletionStoredProcedureExisted())
            {
                CreateDeletionStoredProcedure();
            }
        }
    }
}

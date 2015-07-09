using System;
namespace MagicalStorage.Repository
{
    /// <summary>
    /// This interface defines functions to create database objects (table, store procedure).
    /// </summary>
    public interface IDatabaseCreator
    {
        /// <summary>
        /// Store entity type which is reference to database table
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Check if database table for the entity type exists.
        /// </summary>
        /// <returns>Boolean</returns>
        bool IsDatabaseTableExisted();

        /// <summary>
        /// Create a new database table for entity type (assume it doesn't exist before).
        /// </summary>
        void CreateDatabaseTable();

        /// <summary>
        /// Check database table existance, if not, create a new one.
        /// </summary>
        void CheckExistanceThenCreateDatabaseTable();
    }
}

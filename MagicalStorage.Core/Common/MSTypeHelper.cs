using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class provides static helpers function dealing with Type.
    /// </summary>
    public static class MSTypeHelper
    {
        /// <summary>
        /// Check if a type is primitive, such as Bool, Int, etc
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsPrimitiveType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return type.IsPrimitive;
        }

        /// <summary>
        /// Check if a type is string
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsStringType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return "String".Equals(type.Name);
        }

        /// <summary>
        /// Check if a type is Guid
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsGuidType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return "Guid".Equals(type.Name);
        }

        /// <summary>
        /// Check if a type is DateTime
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsDateTimeType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return "DateTime".Equals(type.Name);
        }

        /// <summary>
        /// Check if a type is one of primitive, string, Guid or DateTime.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSingularType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return IsPrimitiveType(type)
                || IsStringType(type)
                || IsGuidType(type)
                || IsDateTimeType(type);
        }

        /// <summary>
        /// Check if a type is entity type, which will be saved to persist store.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        /// <remarks>Entity type is user defined type which marked with [EntityType] attribute</remarks>
        public static bool IsEntityType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            var attr = Attribute.GetCustomAttribute(type, typeof(EntityTypeAttribute));
            return (attr != null);
        }

        /// <summary>
        /// Check if a type is List<T> where T is any type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsCollectionType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return "List`1".Equals(type.Name);
        }

        /// <summary>
        /// Get generic type of a collection.
        /// </summary>
        /// <param name="type">Collection type</param>
        /// <returns>Type</returns>
        /// <remarks>Return typeof(T) in List<T> type</remarks>
        public static Type GetGenericTypeInCollectionType(Type type)
        {
            Debug.Assert(IsCollectionType(type));

            var genericType = type.GenericTypeArguments[0];
            return genericType;
        }

        /// <summary>
        /// Check if a type is List<T> where T is entity type.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Boolean</returns>
        public static bool IsCollectionOfEntityType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            bool result = false;
            if (IsCollectionType(type))
            {
                var genericType = GetGenericTypeInCollectionType(type);
                if (IsEntityType(genericType))
                {
                    result = true;
                }
            }
            return result;
        }

        public static List<PropertyInfo> GetAllStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    result.Add(propertyInfo);
                }
            }
            return result;
        }

        /// <summary>
        /// Get list of all singular store properties of an entity type (excluding relation).
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>List of property info (always not null, count >= 0)</returns>
        public static List<PropertyInfo> GetAllSingularStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    if (MSTypeHelper.IsSingularType(propertyInfo.PropertyType))
                    {
                        result.Add(propertyInfo);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get all singular relation store properties of a given entity type.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Always not null list of properties (count >= 0)</returns>
        public static List<PropertyInfo> GetAllSingularRelationStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    if (MSTypeHelper.IsEntityType(propertyInfo.PropertyType))
                    {
                        result.Add(propertyInfo);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get all collection relation store properties of a given entity type.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Always not null list of properties (count >= 0)</returns>
        public static List<PropertyInfo> GetAllCollectionRelationStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    if (MSTypeHelper.IsCollectionOfEntityType(propertyInfo.PropertyType))
                    {
                        result.Add(propertyInfo);
                    }
                }
            }
            return result;
        }

        public static List<PropertyInfo> GetAllRelationStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    if (MSTypeHelper.IsCollectionOfEntityType(propertyInfo.PropertyType) || MSTypeHelper.IsEntityType(propertyInfo.PropertyType))
                    {
                        result.Add(propertyInfo);
                    }
                }
            }
            return result;
        }

        public static bool IsProxyType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            return (type.GetProperty(MSConstants.NameOfPrimaryIdProperty) != null);
        }

        public static Type GetBaseType(Type type)
        {
            Debug.Assert(type != null, "Checking not null for type param should be done somewhere else");

            if (IsProxyType(type))
            {
                return type.BaseType;
            }
            return type;
        }
    }
}

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
    /// This class provides static functions to deal with PropertyInfo.
    /// </summary>
    public static class MSPropertyHelper
    {
        /// <summary>
        /// Check if a property type is IList<T>.
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>True if property type is IList / List</returns>
        public static bool IsCollectionProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.Name.Contains("List");
        }

        public static bool IsNotStoreProperty(PropertyInfo propertyInfo)
        {
            return IsXYZProperty(propertyInfo, typeof(NotStoreAttribute));
        }

        public static bool IsStoreProperty(PropertyInfo propertyInfo)
        {
            return !IsNotStoreProperty(propertyInfo)
                && IsPublicReadwriteProperty(propertyInfo);
        }

        public static bool IsReadwriteProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        public static bool IsPublicReadwriteProperty(PropertyInfo propertyInfo)
        {
            var result = false;
            if (IsReadwriteProperty(propertyInfo))
            {
                var getMethodInfo = propertyInfo.GetMethod;
                var setMethodInfo = propertyInfo.SetMethod;
                if (getMethodInfo.IsPublic && setMethodInfo.IsPublic)
                {
                    result = true;
                }
            }
            return result;
        }

        public static bool IsPublicVirtualReadwriteNonAbstractProperty(PropertyInfo propertyInfo)
        {
            var result = false;
            if (propertyInfo.CanRead && propertyInfo.CanWrite)
            {
                var getMethodInfo = propertyInfo.GetMethod;
                var setMethodInfo = propertyInfo.SetMethod;
                if (getMethodInfo.IsPublic && setMethodInfo.IsPublic && setMethodInfo.IsVirtual && !setMethodInfo.IsAbstract)
                {
                    result = true;
                }
            }
            return result;
        }

        public static bool IsVirtualProperty(PropertyInfo propertyInfo)
        {
            var result = false;
            var getMethodInfo = propertyInfo.GetMethod;
            var setMethodInfo = propertyInfo.SetMethod;
            if ((getMethodInfo != null && getMethodInfo.IsVirtual) &&
                (setMethodInfo != null && setMethodInfo.IsVirtual))
            {
                result = true;
            }
            return result;
        }

        public static object GetValueOfPropertyWithName(string propertyName, object entity)
        {
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            return propertyInfo.GetValue(entity);
        }

        public static void SetValueOfPropertyWithName(string propertyName, object entity, object value)
        {
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(entity, value);
        }

        /// <summary>
        /// Check if it is a mandatory property
        /// </summary>
        /// <param name="propertyInfo">propertyInfo must be not NULL; otherwise, exception</param>
        /// <returns>Return true if it is a mandatory property</returns>
        public static bool IsMandatoryProperty(PropertyInfo propertyInfo)
        {
            return IsXYZProperty(propertyInfo, typeof(RequiredAttribute));
        }

        /// <summary>
        /// Get max length of a string property
        /// </summary>
        /// <param name="propertyInfo">pi must be not NULL; otherwise, exception</param>
        /// <returns>Return maxlength of a string property or -1 if it is not String or 255 if not declared</returns>
        public static int GetStringMaxLength(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(MSTypeHelper.IsStringType(propertyInfo.PropertyType));

            var attr = Attribute.GetCustomAttribute(propertyInfo, typeof(StringLengthAttribute)) as StringLengthAttribute;
            if (attr != null)
            {
                return attr.MaxLength;
            }            
            return 255;
        }

        /// <summary>
        /// Get min length of a string property
        /// </summary>
        /// <param name="propertyInfo">pi must be not NULL; otherwise, exception</param>
        /// <returns>Return minlength of a string property or -1 if it is not String or 0 if not declared</returns>
        public static int GetStringMinLength(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(MSTypeHelper.IsStringType(propertyInfo.PropertyType));

            var attr = Attribute.GetCustomAttribute(propertyInfo, typeof(StringLengthAttribute)) as StringLengthAttribute;
            if (attr != null)
            {
                return attr.MinLength;
            }
            return 0;
        }

        private static bool IsXYZProperty(PropertyInfo propertyInfo, Type attributeType)
        {
            var attr = Attribute.GetCustomAttribute(propertyInfo, attributeType);
            return (attr != null);
        }
    }
}

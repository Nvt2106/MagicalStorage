using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Validation
{
    /// <summary>
    /// Validate if one store property is declared correctly.
    /// </summary>
    /// <remarks>
    /// Stored property is a property which is declared as public read-write,
    /// and NOT mark with [NotStore] attribute.
    /// Stored property is valid if it is:
    /// - Valid type (by calling IsValidTypeForStoreProperty function to check)
    /// - It is virtual property if entity or collection of entity
    /// - Name of EntityType property must be the same EntityType name
    /// (e.g. Entity type is Person => name of property must be Person also)
    /// </remarks>
    public class MSPropertyDeclarationValidator : IMSValidator
    {
        public PropertyInfo PropertyInfo { get; private set; }

        public MSPropertyDeclarationValidator(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null, "Checking param propertyInfo not null should be done elsewhere");

            this.PropertyInfo = propertyInfo;
        }

        private bool IsValidTypeForStoreProperty(Type propertyType)
        {
            Debug.Assert(propertyType != null);

            return MSTypeHelper.IsSingularType(propertyType)
                || MSTypeHelper.IsEntityType(propertyType)
                || MSTypeHelper.IsCollectionOfEntityType(propertyType);
        }

        /// <summary>
        /// Check if a store property is defined correctly.
        /// </summary>
        /// <returns>List of MSError (count > 0) or null</returns>        
        public List<MSError> Validate()
        {
            var result = new List<MSError>();

            var propertyType = this.PropertyInfo.PropertyType;
            if (!IsValidTypeForStoreProperty(propertyType))
            {
                result.Add(new MSError(String.Format("Type '{0}' is not valid for property '{1}'", propertyType.Name, this.PropertyInfo.Name)));
            }

            if (MSTypeHelper.IsEntityType(propertyType)
                    || MSTypeHelper.IsCollectionOfEntityType(propertyType))
            {
                if (!MSPropertyHelper.IsVirtualProperty(this.PropertyInfo))
                {
                    result.Add(new MSError(String.Format("Property '{0}' must be declared as virtual", this.PropertyInfo.Name)));
                }
            }

            if (MSTypeHelper.IsEntityType(propertyType)
                && (!propertyType.Name.Equals(this.PropertyInfo.Name)))
            {
                result.Add(new MSError(String.Format("Property '{0}' must be renamed to '{1}'", this.PropertyInfo.Name, propertyType.Name)));
            }

            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }
    }
}

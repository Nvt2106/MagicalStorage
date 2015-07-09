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
    /// Validate entity data.
    /// </summary>
    public class MSEntityDataValidator : IMSValidator
    {
        public IMSEntity Entity { get; private set; }

        public MSEntityDataValidator(IMSEntity entity)
        {
            Debug.Assert(entity != null);
            Debug.Assert(entity.EntityContext != null);

            this.Entity = entity;
        }

        public List<MSError> Validate()
        {
            // Validate each property
            var result = new List<MSError>();
            var entityType = this.Entity.GetType();
            var listOfSingularStoreProperties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entityType);
            foreach (var property in listOfSingularStoreProperties)
            {
                var errors = ValidateSingularStorePropertyDataOfEntity(property);
                if (errors != null)
                {
                    result.AddRange(errors);
                }
            }

            // Validate unique constraints
            var uniqueGroups = GetUniqueGroups(entityType);
            foreach (var uniqueGroup in uniqueGroups)
            {
                // Search in persist storage if any duplicate entity
                var debugInfo = "";
                var propertyNames = uniqueGroup.Value;
                var conditions = new MSConditions();
                foreach (var propertyName in propertyNames)
                {
                    conditions.Add(new MSCondition(propertyName, MSPropertyHelper.GetValueOfPropertyWithName(propertyName, this.Entity)));
                    debugInfo = debugInfo + "," + propertyName;
                }
                debugInfo = debugInfo.Remove(0, 1);

                var list = this.Entity.EntityContext.Search(entityType.BaseType, conditions, new MSPageSetting(1, 1));
                IMSEntity another = null;
                if (list != null)
                {
                    another = list[0] as IMSEntity;
                }

                if ((another != null) && (!another.EntityId.Equals(this.Entity.EntityId)))
                {
                    result.Add(new MSError(String.Format("Duplicate entity for unique groups ({0})", debugInfo)));
                }
            }

            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }

        private List<PropertyInfo> GetAllSingularAndSingularRelationStorePropertiesOfEntityType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking not null for type param should be done somewhere else");

            var result = new List<PropertyInfo>();
            foreach (var propertyInfo in entityType.GetProperties())
            {
                if (MSPropertyHelper.IsStoreProperty(propertyInfo))
                {
                    if (MSTypeHelper.IsSingularType(propertyInfo.PropertyType)
                        || MSTypeHelper.IsEntityType(propertyInfo.PropertyType))
                    {
                        result.Add(propertyInfo);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Validate data for one singular property.
        /// </summary>
        /// <param name="propertyInfo">Property to be validated</param>
        /// <returns>List of MSError (count > 0) o null if not error</returns>
        private List<MSError> ValidateSingularStorePropertyDataOfEntity(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null);
            
            object value = propertyInfo.GetValue(this.Entity);
            List<MSError> result = null;
            if (MSTypeHelper.IsStringType(propertyInfo.PropertyType))
            {
                result = ValidateStringProperty(propertyInfo, (string)value);
            }
            else if (MSTypeHelper.IsGuidType(propertyInfo.PropertyType))
            {
                result = ValidateGuidProperty(propertyInfo, (Guid)value);
            }
            else if (MSTypeHelper.IsDateTimeType(propertyInfo.PropertyType))
            {
                result = ValidateDateTimeProperty(propertyInfo, (DateTime)value);
            }

            return result;
        }

        private List<MSError> ValidateDateTimeProperty(PropertyInfo propertyInfo, DateTime value)
        {
            List<MSError> result = null;
            if (MSPropertyHelper.IsMandatoryProperty(propertyInfo))
            {
                if (DateTime.MinValue.Equals(value))
                {
                    result = new List<MSError>();
                    result.Add(new MSError(String.Format("Property '{0}' is required", propertyInfo.Name)));
                }
            }
            return result;
        }

        private List<MSError> ValidateGuidProperty(PropertyInfo propertyInfo, Guid value)
        {
            List<MSError> result = null;
            if (MSPropertyHelper.IsMandatoryProperty(propertyInfo) && Guid.Empty.Equals(value))
            {
                result = new List<MSError>();
                result.Add(new MSError(String.Format("Property '{0}' is required", propertyInfo.Name)));
            }
            return result;
        }

        private List<MSError> ValidateStringProperty(PropertyInfo propertyInfo, string value)
        {
            var result = new List<MSError>();
            if (MSPropertyHelper.IsMandatoryProperty(propertyInfo) && String.IsNullOrWhiteSpace(value))
            {
                result.Add(new MSError(String.Format("Property '{0}' is required", propertyInfo.Name)));
            }

            // Check Max length
            int maxLength = MSPropertyHelper.GetStringMaxLength(propertyInfo);
            if (maxLength > 0)
            {
                if (!String.IsNullOrWhiteSpace(value) && (maxLength < value.Length))
                {
                    result.Add(new MSError(String.Format("Length of property '{0}' is exceed {1}", propertyInfo.Name, maxLength)));
                }
            }

            // Check Min length
            int minLength = MSPropertyHelper.GetStringMinLength(propertyInfo);
            if (minLength >= 0)
            {
                int stringLength = MSStringHelper.LengthOfString(value);
                if (minLength > stringLength)
                {
                    result.Add(new MSError(String.Format("Length of property '{0}' is less than {1}", propertyInfo.Name, minLength)));
                }
            }

            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }

        private Dictionary<string, List<string>> GetUniqueGroups(Type entityType)
        {
            var listOfSingularStoreProperties = GetAllSingularAndSingularRelationStorePropertiesOfEntityType(entityType);
            var uniqueGroups = new Dictionary<string, List<string>>(); // List<string> = list of properties in group
            foreach (var property in listOfSingularStoreProperties)
            {
                var uniqueAttribute = Attribute.GetCustomAttribute(property, typeof(UniqueAttribute)) as UniqueAttribute;
                if (uniqueAttribute != null)
                {
                    var groupName = uniqueAttribute.GroupName;
                    string propertyName = "";
                    if (MSTypeHelper.IsSingularType(property.PropertyType))
                    {
                        propertyName = property.Name;
                    }
                    else
                    {
                        propertyName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(property.Name);
                    }

                    if (!uniqueGroups.ContainsKey(groupName))
                    {
                        uniqueGroups.Add(groupName, new List<string> { propertyName });
                    }
                    else
                    {
                        var list = uniqueGroups[groupName];
                        list.Add(propertyName);
                        uniqueGroups[groupName] = list;
                    }
                }
            }

            return uniqueGroups;
        }
    }
}

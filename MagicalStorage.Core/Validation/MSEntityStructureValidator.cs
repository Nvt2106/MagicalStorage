using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Validation
{
    /// <summary>
    /// Validate an entity type if all its properties are declared correctly.
    /// </summary>
    public class MSEntityStructureValidator : IMSValidator
    {
        public Type EntityType { get; private set; }
        public List<Type> AllEntityTypes { get; private set; }

        public MSEntityStructureValidator(Type entityType, List<Type> allEntityTypes)
        {
            Debug.Assert(entityType != null);
            Debug.Assert(MSTypeHelper.IsEntityType(entityType));
            Debug.Assert(allEntityTypes != null);
            Debug.Assert(allEntityTypes.Contains(entityType));

            this.EntityType = entityType;
            this.AllEntityTypes = allEntityTypes;
        }

        public List<MSError> Validate()
        {
            var result = new List<MSError>();
            var storeProperties = MSTypeHelper.GetAllStorePropertiesOfEntityType(this.EntityType);
            foreach (var property in storeProperties)
            {
                var propertyDeclarationValidator = new MSPropertyDeclarationValidator(property);
                var errors = propertyDeclarationValidator.Validate();
                if (errors != null)
                {
                    result.AddRange(errors);
                }

                // Check entity property type if it is in list allEntityTypes
                var propertyType = property.PropertyType;
                if (MSTypeHelper.IsEntityType(propertyType))
                {
                    if (!this.AllEntityTypes.Contains(propertyType))
                    {
                        result.Add(new MSError(String.Format("Type '{0}' of property '{1}' does not exist in entity context", propertyType.Name, property.Name)));
                    }
                }
                else if (MSTypeHelper.IsCollectionOfEntityType(propertyType))
                {
                    var genericType = MSTypeHelper.GetGenericTypeInCollectionType(propertyType);
                    if (!this.AllEntityTypes.Contains(genericType))
                    {
                        result.Add(new MSError(String.Format("Type '{0}' of property '{1}' does not exist in entity context", genericType.Name, property.Name)));
                    }
                }
            }
            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }
    }
}

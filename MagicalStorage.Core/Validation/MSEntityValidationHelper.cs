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
    /// This class provides static functions to help validating entity type and properties and its data.
    /// </summary>
    public static class MSEntityValidationHelper
    {
        /// <summary>
        /// Validate list of entity types to see if any type in list is invalid.
        /// </summary>
        /// <param name="listEntityTypes">List of entity types to validate</param>
        /// <remarks>
        /// Throw MSException if any type is invalid, instead of returning list of errors,
        /// since invalid type will lead to wrong application running,
        /// and developer must to correct type declaration to go futher.
        /// </remarks>
        public static void ValidateStructureForListEntityTypes(List<Type> listEntityTypes)
        {
            Debug.Assert(listEntityTypes != null);

            foreach (var type in listEntityTypes)
            {
                var validator = new MSEntityStructureValidator(type, listEntityTypes);
                var errors = validator.Validate();
                if (errors != null)
                {
                    throw new MSException(String.Format("Type '{0}' is not declared correctly. Check MSException.Errors for details.", type.Name), errors);
                }
            }
        }

        
        /// <summary>
        /// Validate entity data.
        /// </summary>
        /// <param name="entity">Entity data</param>
        /// <returns>List of MSError (count > 0) o null if not error</returns>
        /// <remarks>
        /// It validates only data for singular store properties.
        /// For relation entity, it will be validated when saving relation entity data.
        /// </remarks>
        public static List<MSError> ValidateDataOfEntity(IMSEntity entity)
        {
            var validator = new MSEntityDataValidator(entity);
            return validator.Validate();
        }
    }
}

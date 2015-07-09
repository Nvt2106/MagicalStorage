using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public static class MSNameHelper
    {
        public static string NameOfAssemblyForType(Type entityType)
        {
            return "MSAssembly_" + entityType.Assembly.GetName().Name;
        }

        public static string NameOfModuleForType(Type entityType)
        {
            return "MSModule_" + entityType.Module.Name;
        }

        public static string NameOfLoadDataMethodForRelationProperty(string propertyName)
        {
            return "LoadData_" + propertyName;
        }

        public static string NameOfSingularPropertyIdForSingularRelationProperty(string propertyName)
        {
            return propertyName + "Id";
        }

        public static string NameOfIsLoadedPropertyForRelationProperty(string propertyName)
        {
            return "IsLoaded_" + propertyName;
        }

        public static string PreviousPropertyNameForStoreProperty(string propertyName)
        {
            return "Previous_" + propertyName;
        }

        public static string NameOfSetMethodForProperty(string propertyName)
        {
            return "set_" + propertyName;
        }

        public static string NameOfGetMethodForProperty(string propertyName)
        {
            return "get_" + propertyName;
        }

        public static string NameOfBackingPrivateFieldForProperty(string propertyName)
        {
            return "backingPrivate_" + propertyName;
        }

        public static string NameOfReversedPropertyToType(Type entityType)
        {
            return entityType.Name;
        }
    }
}

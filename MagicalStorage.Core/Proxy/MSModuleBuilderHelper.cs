using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides static function to get module builder for a given type.
    /// </summary>
    public static class MSModuleBuilderHelper
    {
        /// <summary>
        /// Get module builder for a given type.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>ModuleBuilder</returns>
        /// <remarks>
        /// This function gets module builder from cache first;
        /// if not exists, then create a new module builder, and add it to cache.
        /// </remarks>
        public static ModuleBuilder GetModuleBuilderForType(Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            
            var assemblyName = MSNameHelper.NameOfAssemblyForType(entityType);
            var moduleName = MSNameHelper.NameOfModuleForType(entityType);
            var moduleBuilderCachedKey = assemblyName + moduleName;

            // Try to get module builder from cache first
            var moduleBuilder = MSModuleBuilderCaching.Instance.GetCachedObjectForKey(moduleBuilderCachedKey);

            // If not exist in cache, then create a new one and add it to cache
            if (moduleBuilder == null)
            {
                var moduleGenerator = new MSModuleGenerator(assemblyName, moduleName);
                moduleBuilder = moduleGenerator.GetModuleBuilder();
                MSModuleBuilderCaching.Instance.CacheObject(moduleBuilder, moduleBuilderCachedKey);
            }

            return moduleBuilder;
        }
    }
}

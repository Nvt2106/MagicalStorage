using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides functions to build Assembly and Module dynamic.
    /// </summary>
    public class MSModuleGenerator
    {
        // Name of assembly
        public string AssemblyName { get; private set; }

        // Name of module
        public string ModuleName { get; private set; }
        
        // Assembly builder
        private AssemblyBuilder assemblyBuilder;

        // Module builder
        private ModuleBuilder moduleBuilder;
        
        /// <summary>
        /// Constructor to create instance of this class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="moduleName">Module name</param>
        /// <remarks>
        /// All param must be not null; otherwise, exception is thrown.
        /// </remarks>
        public MSModuleGenerator(string assemblyName, string moduleName)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(assemblyName, "assemblyName");
            MSParameterHelper.MakeSureInputParameterNotNull(moduleName, "moduleName");
            
            this.AssemblyName = assemblyName;
            this.ModuleName = moduleName;

            assemblyBuilder = BuildAssembly();
            moduleBuilder = BuildModule();
        }

        /// <summary>
        /// Build an assembly.
        /// </summary>
        /// <returns>AssemblyBuilder</returns>
        private AssemblyBuilder BuildAssembly()
        {
            var assemblyNameObject = new AssemblyName(this.AssemblyName);
            assemblyNameObject.Version = new Version(1, 0, 0, 0);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyNameObject, AssemblyBuilderAccess.RunAndSave);
            return assemblyBuilder;
        }

        /// <summary>
        /// Build a module.
        /// </summary>
        /// <returns>ModuleBuilder</returns>
        private ModuleBuilder BuildModule()
        {
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(this.ModuleName);
            return moduleBuilder;
        }

        /// <summary>
        /// Return assembly.
        /// </summary>
        /// <returns>AssemblyBuilder</returns>
        public AssemblyBuilder GetAssemblyBuilder()
        {
            return assemblyBuilder;
        }

        /// <summary>
        /// Return module builder.
        /// </summary>
        /// <returns>ModuleBuilder</returns>
        public ModuleBuilder GetModuleBuilder()
        {
            return moduleBuilder;
        }
    }
}

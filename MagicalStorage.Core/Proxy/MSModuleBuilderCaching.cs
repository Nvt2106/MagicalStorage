using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides simple caching functions for module builder.
    /// </summary>
    public sealed class MSModuleBuilderCaching : MSSimpleCache<ModuleBuilder>
    {
        private static readonly MSModuleBuilderCaching instance = new MSModuleBuilderCaching();

        private MSModuleBuilderCaching() { }

        /// <summary>
        /// Return singleton instance of MSModuleBuilderCaching.
        /// </summary>
        public static MSModuleBuilderCaching Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

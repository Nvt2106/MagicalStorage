using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides simple caching function for Type instance.
    /// </summary>
    public sealed class MSTypeCaching : MSSimpleCache<Type>
    {
        private static readonly MSTypeCaching instance = new MSTypeCaching();

        private MSTypeCaching() { }

        /// <summary>
        /// Return singleton instance of MSTypeCaching.
        /// </summary>
        public static MSTypeCaching Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

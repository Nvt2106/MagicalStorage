using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class provides simple functions for caching an generic object and get object from cache by key.
    /// </summary>
    public class MSSimpleCache<T>
    {
        // Store all cached objects
        public Dictionary<string, T> cachedObjects = new Dictionary<string, T>();

        /// <summary>
        /// Retrieve an object from cache.
        /// </summary>
        /// <param name="key">A string to name that object in cache</param>
        /// <returns>Object</returns>
        /// <remarks>
        /// Param key must be not null or empty or all whitespaces; otherwise, exception is thrown.
        /// Function returns null if not found any cache for given key.
        /// </remarks>
        public T GetCachedObjectForKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new MSException("Param key must be not null or empty or all whitespaces");
            }

            if (cachedObjects.ContainsKey(key))
            {
                return cachedObjects[key];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Save an object to cache with a given key.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="key">Given key to identify object in cache</param>
        /// <remarks>
        /// Param obj must be not null; otherwise, exception is thrown.
        /// Param key must be not null or empty or all whitespaces; otherwise, exception is thrown.
        /// If key exists, this function will override existing value in that key.
        /// If key not exist, this function will add new key to cache.
        /// </remarks>
        public void CacheObject(T obj, string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new MSException("Param key must be not null or empty or all whitespaces");
            }
            if (obj == null)
            {
                throw new MSException("Param obj must be not null");
            }

            cachedObjects[key] = obj;
        }

        public void ClearCache()
        {
            cachedObjects.Clear();
        }
    }
}

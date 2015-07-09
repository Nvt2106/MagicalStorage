using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    /// <summary>
    /// This class provides static helper function to retrieve a proxy type for a given type.
    /// </summary>
    public class MSProxyTypeFactory
    {
        /// <summary>
        /// Check if system can create a proxy type for a given type.
        /// </summary>
        /// <param name="entityType">Given entity type to be checked</param>
        /// <returns>Boolean</returns>
        /// <remarks>
        /// A type can have proxy type if they are inheritable (i.e. not sealed), public class, not abstract.
        /// </remarks>
        public bool CanCreateProxyTypeForType(Type entityType)
        {
            Debug.Assert(entityType != null, "Checking param entityType not null should be done before calling this function");

            return entityType.IsClass 
                && entityType.IsPublic 
                && !entityType.IsAbstract 
                && !entityType.IsSealed;
        }

        /// <summary>
        /// Get proxy type of a given type.
        /// It retrieves cache if exist, otherwise, create a new proxy type and save to cache.
        /// </summary>
        /// <param name="entityType">Type of entity which needs to have proxy type.</param>
        /// <param name="parentEntityTypes">List of parent types which this entity type is child</param>
        /// <returns>Proxy type, including all proxy properties</returns>
        /// <remarks>
        /// Param entityType must be not null; otherwise, exception is thrown.
        /// Given type must be inheritable; otherwise, exception is thrown.
        /// This function is virtual to support unit testing.
        /// </remarks>
        public virtual Type GetProxyTypeForType(Type entityType, List<Type> parentEntityTypes = null)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!CanCreateProxyTypeForType(entityType))
            {
                throw new MSProxyException("Param entityType must be public / class / not sealed / not abstract");
            }

            // Try to get proxy type from cache first
            var cachedProxyTypeKey = GetProxyTypeKeyForCaching(entityType);
            var proxyType = MSTypeCaching.Instance.GetCachedObjectForKey(cachedProxyTypeKey);

            // If not exist in cache, then create a new one and add it to cache
            if (proxyType == null)
            {
                var moduleBuilder = MSModuleBuilderHelper.GetModuleBuilderForType(entityType);
                var proxyTypeGenerator = new MSProxyTypeGenerator(moduleBuilder, entityType, parentEntityTypes);
                proxyType = proxyTypeGenerator.GetProxyType();

                MSTypeCaching.Instance.CacheObject(proxyType, cachedProxyTypeKey);
            }

            return proxyType;
        }

        private string GetProxyTypeKeyForCaching(Type entityType)
        {
            return entityType.FullName;
        }

        /// <summary>
        /// Get list of proxy types for given list of entity types.
        /// </summary>
        /// <param name="types">List of entity types</param>
        /// <returns>List of proxy types</returns>
        /// <remarks>This function calls GetProxyTypeForType to get proxy type for each entity type</remarks>
        public List<Type> GetProxyTypesForTypes(List<Type> types)
        {
            Debug.Assert(types != null);
            Debug.Assert(types.Count > 0);

            // Prepare reversed dictionary for all types
            // In case an entity type has collection relation to another type,
            // type in collection must have reversed singular relation to entity type
            // E.g.: Parent type has collection property List<Child>,
            //       then, Child must have singular property Parent Parent
            // This dictionary stores pair: Child type (key) and list of parent types (value),
            // so that, when creating proxy type for Child, it will creates those reversed if necessary
            // (necessary means developer not yet define it)
            var reversedRelations = PrepareReversedRelationInfo(types);

            // Then create proxy types for those types with reversed relation property
            var result = new List<Type>();
            foreach (var type in types)
            {
                List<Type> parentEntityTypes = null;
                if (reversedRelations.ContainsKey(type))
                {
                    parentEntityTypes = reversedRelations[type];
                }
                var proxyType = GetProxyTypeForType(type, parentEntityTypes);
                result.Add(proxyType);
            }
            reversedRelations = null;

            return result;
        }

        private Dictionary<Type, List<Type>> PrepareReversedRelationInfo(List<Type> types)
        {
            var reversedRelations = new Dictionary<Type, List<Type>>();
            foreach (var type in types)
            {
                var listOfCollectionRelationProperties = MSTypeHelper.GetAllCollectionRelationStorePropertiesOfEntityType(type);
                foreach (var relationProperty in listOfCollectionRelationProperties)
                {
                    var relationType = MSTypeHelper.GetGenericTypeInCollectionType(relationProperty.PropertyType);
                    if (!reversedRelations.ContainsKey(relationType))
                    {
                        reversedRelations.Add(relationType, new List<Type>());
                    }

                    var listEntityTypes = reversedRelations[relationType];
                    if (!listEntityTypes.Contains(type))
                    {
                        // Check if relation type already have reversed relation to parent type
                        var listOfSingularRelationProperties = MSTypeHelper.GetAllSingularRelationStorePropertiesOfEntityType(relationType);
                        var alreadyDefined = false;
                        foreach (var singularRelationProperty in listOfSingularRelationProperties)
                        {
                            // TODO: in this version, require reversed relation name must be the same name of parent type
                            if (type.Equals(singularRelationProperty.PropertyType))
                            {
                                if (singularRelationProperty.Name.Equals(MSNameHelper.NameOfReversedPropertyToType(type)))
                                {
                                    alreadyDefined = true;
                                    break;
                                }
                            }
                        }
                        if (!alreadyDefined)
                        {
                            listEntityTypes.Add(type);
                        }                        
                    }
                }
            }
            return reversedRelations;
        }
    }
}

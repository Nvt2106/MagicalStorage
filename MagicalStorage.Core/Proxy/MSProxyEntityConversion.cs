using MagicalStorage.Core.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class provides functions to convert entity to proxy entity.
    /// </summary>
    public class MSProxyEntityConversion
    {
        // Store entity context
        public MSEntityContext EntityContext { get; private set; }

        // Store entities which are already converted and their proxy entities (completed)
        private Dictionary<object, object> dictProcessedEntity = new Dictionary<object, object>();

        // Store entities which are converting and their proxy entities (not yet completed)
        private Dictionary<object, object> dictProcessingEntity = new Dictionary<object, object>();

        /// <summary>
        /// Constructor to instantiate object for this class.
        /// </summary>
        /// <param name="entityContext">Entity context</param>
        public MSProxyEntityConversion(MSEntityContext entityContext)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityContext, "entityContext");
            
            this.EntityContext = entityContext;
        }

        /// <summary>
        /// Convert entity to proxy entity, including any relation entity.
        /// </summary>
        /// <param name="entity">Entity to be converted</param>
        /// <returns>
        /// Proxy entity (which implement IMSEntity interface)
        /// </returns>
        /// <remarks>
        /// Type of entity must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// Param entity must be not null; otherwise, exception is thrown.
        /// </remarks>
        public IMSEntity ConvertEntityToProxyEntity(object entity)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");
            if (!this.EntityContext.IsTypeManagedByThisContext(MSTypeHelper.GetBaseType(entity.GetType())))
            {
                throw new MSException("Generic type T must be type managed by the entity context");
            }

            dictProcessedEntity.Clear();
            dictProcessingEntity.Clear();
            
            IMSEntity result;
            DoConversion(entity, out result);
            
            dictProcessedEntity.Clear();
            dictProcessingEntity.Clear();

            return result;
        }

        private void DoConversion(object entity, out IMSEntity result)
        {
            // Check if this entity is converted already, just return it
            object proxyEntity;
            if (dictProcessedEntity.TryGetValue(entity, out proxyEntity))
            {
                result = (IMSEntity)proxyEntity;
                return;
            }

            // Otherwise, do convert for entity itself and all its relation entities
            // First prepare proxy entity
            var entityType = entity.GetType();
            Type proxyType;
            if (MSTypeHelper.IsProxyType(entityType))
            {
                proxyType = entityType;
                result = (IMSEntity)entity;
            }
            else
            {
                result = CreateAndPrepareProxyEntityForEntity(entity, out proxyType);                
            }

            // Mark this entity as in processing, so that when there is a relation to this entity later,
            // proxy entity of this entity will retrieve from this cache, instead of creating a new one
            if (!dictProcessingEntity.ContainsKey(entity))
            {
                dictProcessingEntity.Add(entity, result);
            }
            
            // Convert singular relation entities
            var singularRelationProperties = MSTypeHelper.GetAllSingularRelationStorePropertiesOfEntityType(entityType);
            foreach (var singularRelationProperty in singularRelationProperties)
            {
                var isLoaded = MSProxyEntityConversion.IsRelationPropertyLoaded(singularRelationProperty.Name, result);
                if (isLoaded)
                {
                    DoConversionForSingularRelationProperty(entity, singularRelationProperty, result);
                }                
            }
            
            // Convert collection relation entity
            var collectionRelationProperties = MSTypeHelper.GetAllCollectionRelationStorePropertiesOfEntityType(entityType);
            foreach (var collectionRelationProperty in collectionRelationProperties)
            {
                // Only convert if it is new entity, or
                // proxy entity which already loaded data from persist storage
                // for the case not yet loaded, mean that it is not changed, so don't need to convert
                var isLoaded = MSProxyEntityConversion.IsRelationPropertyLoaded(collectionRelationProperty.Name, result);
                if (isLoaded)
                {
                    var collectionRelationEntity = (IList)collectionRelationProperty.GetValue(entity); // List<RelationEntity> or List<ProxyRelationEntity>
                    if (collectionRelationEntity != null)
                    {
                        var collectionRelationProxyEntity = (IList)Activator.CreateInstance(collectionRelationEntity.GetType());

                        // Convert and add processed entity to this List
                        foreach (object relationEntity in collectionRelationEntity)
                        {
                            IMSEntity proxyRelationEntity;
                            DoConversion(relationEntity, out proxyRelationEntity);

                            collectionRelationProxyEntity.Add(proxyRelationEntity);

                            // Make sure reverse relation back from proxy relation entity to this proxy entity
                            var entityTypeName = proxyType.Name.Substring(MSConstants.PrefixNameForProxyType.Length);
                            var reversedPropertyIdName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(entityTypeName);
                            var reversedPropertyId = proxyRelationEntity.GetType().GetProperty(reversedPropertyIdName);
                            reversedPropertyId.SetValue(proxyRelationEntity, result.EntityId);

                            var reversedProperty = proxyRelationEntity.GetType().GetProperty(entityTypeName);
                            reversedProperty.SetValue(proxyRelationEntity, result);
                        }

                        var collectionRelationPropertyOnProxy = proxyType.GetProperty(collectionRelationProperty.Name);
                        collectionRelationPropertyOnProxy.SetValue(result, collectionRelationProxyEntity);
                    }                    
                }                
            }
            
            // Mark this entity as processed already
            if (!dictProcessedEntity.ContainsKey(entity))
            {
                dictProcessedEntity.Add(entity, result);
            }            
        }

        // entity is not proxy entity,
        // this is asure when calling this function from DoConversion()
        private IMSEntity CreateAndPrepareProxyEntityForEntity(object entity, out Type proxyType)
        {
            // Create new proxy entity instance
            var entityType = entity.GetType();
            var result = CreateNewProxyEntityInstance(entityType, out proxyType);

            // Set value for IMSEntity properties
            result.EntityId = Guid.NewGuid();
            result.EntityContext = this.EntityContext;

            // Copy value for all singular properties
            CopyAllSingularPropertyDataToProxyEntity(entity, result);

            // Mark all relations are already loaded
            // Because in case entity is already proxy, and not yet load relation,
            // we don't need to convert relation because it is not changed
            // Later in this method, we will check only if it is loaded, then do conversion
            var relationProperties = MSTypeHelper.GetAllRelationStorePropertiesOfEntityType(entityType);
            foreach (var relationProperty in relationProperties)
            {
                SetIsRelationPropertyLoaded(relationProperty.Name, result, true);
            }

            return result;
        }

        private IMSEntity CreateNewProxyEntityInstance(Type entityType, out Type proxyType)
        {
            proxyType = GetProxyTypeFromListProxyTypesForType(entityType, this.EntityContext.ProxyEntityTypes);
            var result = (IMSEntity)Activator.CreateInstance(proxyType);
            return result;
        }

        // entity is not proxy entity,
        // this is asure when calling this function from CreateAndPrepareProxyEntityForEntity()
        private static void CopyAllSingularPropertyDataToProxyEntity(object entity, IMSEntity proxyEntity)
        {
            var allSingularProperties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entity.GetType());
            foreach (var propertyInfo in allSingularProperties)
            {
                var value = propertyInfo.GetValue(entity);
                MSPropertyHelper.SetValueOfPropertyWithName(propertyInfo.Name, proxyEntity, value);
            }
        }

        private void DoConversionForSingularRelationProperty(object entity, PropertyInfo singularRelationProperty, IMSEntity proxyEntity)
        {
            var proxyType = proxyEntity.GetType();
            var relationEntity = singularRelationProperty.GetValue(entity);
            if (relationEntity != null)
            {
                object proxyRelationEntity;
                if (!dictProcessingEntity.TryGetValue(relationEntity, out proxyRelationEntity))
                {
                    IMSEntity proxyRelationEntity2;
                    DoConversion(relationEntity, out proxyRelationEntity2);
                    proxyRelationEntity = proxyRelationEntity2;
                }

                var singularRelationPropertyInProxyEntity = proxyType.GetProperty(singularRelationProperty.Name);
                singularRelationPropertyInProxyEntity.SetValue(proxyEntity, proxyRelationEntity);

                // And set value for relation Id
                var relationIdPropertyName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(singularRelationProperty.Name);
                var relationIdProperty = proxyType.GetProperty(relationIdPropertyName);
                var relationId = ((IMSEntity)proxyRelationEntity).EntityId;
                relationIdProperty.SetValue(proxyEntity, relationId);
            }            
        }

        private Type GetProxyTypeFromListProxyTypesForType(Type entityType, List<Type> proxyTypes)
        {
            var proxyTypeName = MSConstants.PrefixNameForProxyType + entityType.Name;
            foreach (var proxyType in proxyTypes)
            {
                if (proxyType.Name.Equals(proxyTypeName))
                {
                    return proxyType;
                }
            }
            return null; // this code should never reach!!!
        }


        #region static helper functions

        private static PropertyInfo GetIsLoadedPropertyForRelationProperty(string propertyName, Type proxyType)
        {
            var isLoadedPropertyName = MSNameHelper.NameOfIsLoadedPropertyForRelationProperty(propertyName);
            var isLoadedProperty = proxyType.GetProperty(isLoadedPropertyName);
            return isLoadedProperty;
        }

        public static bool IsRelationPropertyLoaded(string propertyName, object proxyEntity)
        {
            var isLoadedProperty = GetIsLoadedPropertyForRelationProperty(propertyName, proxyEntity.GetType());
            var isLoaded = (bool)isLoadedProperty.GetValue(proxyEntity);
            return isLoaded;
        }

        private void SetIsRelationPropertyLoaded(string propertyName, object proxyEntity, bool isLoaded)
        {
            var isLoadedProperty = GetIsLoadedPropertyForRelationProperty(propertyName, proxyEntity.GetType());
            isLoadedProperty.SetValue(proxyEntity, isLoaded);
        }

        #endregion
    }
}

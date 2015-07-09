using MagicalStorage.Core.Proxy;
using MagicalStorage.Core.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class is main class of framework, which provides functions to communicate with persist storage.
    /// Developer defines any entity class to store data, and use this class to save data to persist storage as well as retrieve it.
    /// This class is expensive to instantiate, so it is better to create one and keep it as long as possible.
    /// </summary>
    /// <remarks>
    /// IMPORTANT NOTE:
    /// ===============
    /// This currently NOT support Many-Many relationship among entities.
    /// In that case, developer should break that relationship to 2 One-Many relationships when defining entities.
    /// </remarks>
    public class MSEntityContext
    {
        public IMSRepository Repository { get; private set; } // Repository (persist storage) which entity will be stored

        public List<Type> EntityTypes { get; private set; } // List of entity types managed by this context

        public List<Type> ProxyEntityTypes { get; private set; } // List of proxy types which derived from entity types

        // To create / get proxy types
        private MSProxyTypeFactory proxyTypeFactory;

        // To convert entity to proxy entity before saving
        private MSProxyEntityConversion proxyEntityConversion;

        // Store entities which are saving
        private List<IMSEntity> savingEntities = new List<IMSEntity>();

        #region MSEntityContext constructor

        /// <summary>
        /// Constructor to instantiate this entity context.
        /// </summary>
        /// <param name="repository">IMSRepository</param>
        /// <param name="entityType">The first entity type</param>
        /// <param name="entityTypes">Array of entity types</param>
        /// <remarks>
        /// Param repository must be not null; otherwise, exception is thrown.
        /// Param entityType must not be null; otherwise, exception is thrown.
        /// This constructor also validates all entity types structure, and throw exception if any invalid declaration.
        /// </remarks>
        public MSEntityContext(IMSRepository repository, Type entityType, params Type[] entityTypes)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(repository, "repository");
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            
            this.Repository = repository;

            KeepEntityTypeParamsInPrivateVariable(entityType, entityTypes);

            MSEntityValidationHelper.ValidateStructureForListEntityTypes(this.EntityTypes);

            GenerateProxyTypesForEntityTypes();

            PreparePersistStorageForAllProxyEntityTypes();
        }

        private void KeepEntityTypeParamsInPrivateVariable(Type entityType, Type[] entityTypes)
        {
            this.EntityTypes = new List<Type>();
            KeepOneEntityTypeInPrivateVariable(entityType);
            if (entityTypes != null)
            {
                for (int i = 0; i < entityTypes.Length; i++)
                {
                    KeepOneEntityTypeInPrivateVariable(entityTypes[i]);
                }
            }            
        }

        private void KeepOneEntityTypeInPrivateVariable(Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");

            if (!MSTypeHelper.IsEntityType(entityType))
            {
                throw new MSException(String.Format("Class '{0}' is not entity type (marked with [EntityType] attribute)", entityType.Name));
            }
            if (this.EntityTypes.Contains(entityType))
            {
                throw new MSException(String.Format("Entity '{0}' is duplicated", entityType.Name));
            }

            this.EntityTypes.Add(entityType);
        }

        private void GenerateProxyTypesForEntityTypes()
        {
            proxyTypeFactory = new MSProxyTypeFactory();
            this.ProxyEntityTypes = proxyTypeFactory.GetProxyTypesForTypes(this.EntityTypes);
        }

        private void PreparePersistStorageForAllProxyEntityTypes()
        {
            foreach (var proxyType in this.ProxyEntityTypes)
            {
                try
                {
                    this.Repository.PreparePersistStorageForEntityType(proxyType);
                }
                catch (Exception exp)
                {
                    throw new MSException(String.Format("Failed to prepare storage for entity type '{0}'.", proxyType.Name), exp);
                }
            }            
        }

        #endregion


        #region Get data from repository <T>

        /// <summary>
        /// Get entity from persist storage by its Id.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entityId">Unique Id of entity to get</param>
        /// <returns>Entity instance or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function should not be public, since it is used only by framework (in MSProxyTypeGenerator).
        /// However, it is public to support unit testing.
        /// Developer should NOT use this function, please use Search function instead.
        /// </remarks>
        public virtual T Get<T>(Guid entityId) where T : new()
        {
            return (T)Get(typeof(T), entityId);
        }

        /// <summary>
        /// Get entity from persist storage by its Id.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entityId">Unique Id of entity to get</param>
        /// <returns>Entity instance or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function should not be public, since it is used only by framework (in MSProxyTypeGenerator).
        /// However, it is public to support unit testing.
        /// Developer should NOT use this function, please use Search function instead.
        /// </remarks>
        public virtual object Get(Type entityType, Guid entityId)
        {
            var conditions = new MSConditions();
            conditions.Add(MSConstants.NameOfPrimaryIdProperty, entityId);
            var searchResult = Search(MSTypeHelper.GetBaseType(entityType), conditions, new MSPageSetting());
            return GetOneFromList(searchResult);            
        }

        /// <summary>
        /// Get all entity.
        /// </summary>
        /// <typeparam name="T">Type of entity to get</typeparam>
        /// <returns>List of all entities (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public List<T> GetAll<T>() where T : new()
        {
            return Search<T>(new MSConditions(), new MSPageSetting());
        }

        /// <summary>
        /// Search entity by a single condition.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="condition">A single search condition</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public List<T> Search<T>(MSCondition condition) where T : new()
        {
            var conditions = new MSConditions();
            conditions.Add(condition);
            return Search<T>(conditions);
        }

        /// <summary>
        /// Get one entity by a single condition.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="condition">A single search condition</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public T GetOne<T>(MSCondition condition) where T : new()
        {
            var list = Search<T>(condition, new MSPageSetting(1, 1));
            return GetOneFromList<T>(list);
        }

        private T GetOneFromList<T>(List<T> list)
        {
            if (list != null)
            {
                return list[0];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Search entity by a single condition with paging.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="condition">A single search condition</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public List<T> Search<T>(MSCondition condition, MSPageSetting pageSetting) where T : new()
        {
            var conditions = new MSConditions();
            conditions.Add(condition);
            return Search<T>(conditions, pageSetting);
        }

        /// <summary>
        /// Get one entity by a single condition with paging.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="condition">A single search condition</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>Entity matched with condition or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public T GetOne<T>(MSCondition condition, MSPageSetting pageSetting) where T : new()
        {
            pageSetting.PageSize = 1;
            var list = Search<T>(condition, pageSetting);
            return GetOneFromList<T>(list);
        }

        /// <summary>
        /// Search entity by a multiple conditions.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="conditions">Multiple conditions</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public List<T> Search<T>(MSConditions conditions) where T : new()
        {
            var pageSetting = new MSPageSetting();
            return Search<T>(conditions, pageSetting);
        }

        /// <summary>
        /// Get one entity by a multiple conditions.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="conditions">Multiple conditions</param>
        /// <returns>Entity matched with condition or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// </remarks>
        public T GetOne<T>(MSConditions conditions) where T : new()
        {
            var list = Search<T>(conditions, new MSPageSetting(1, 1));
            return GetOneFromList<T>(list);
        }

        /// <summary>
        /// Search entity by a multiple conditions with paging.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="conditions">Multiple conditions</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// This function is virtual to support unit testing.
        /// </remarks>
        public virtual List<T> Search<T>(MSConditions conditions, MSPageSetting pageSetting) where T : new()
        {
            var list = Search(typeof(T), conditions, pageSetting);
            if (list != null)
            {
                var result = new List<T>();
                foreach (var obj in list)
                {
                    result.Add((T)obj);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get one entity by a multiple conditions with paging.
        /// </summary>
        /// <typeparam name="T">Type of entity to search</typeparam>
        /// <param name="conditions">Multiple conditions</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>Entity matched with condition or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// This function is virtual to support unit testing.
        /// </remarks>
        public virtual T GetOne<T>(MSConditions conditions, MSPageSetting pageSetting) where T : new()
        {
            pageSetting.PageSize = 1;
            var list = Search<T>(conditions, pageSetting);
            return GetOneFromList<T>(list);
        }

        public List<object> Search(Type entityType)
        {
            return Search(entityType, new MSConditions(), new MSPageSetting());
        }

        public List<object> Search(Type entityType, MSConditions conditions)
        {
            return Search(entityType, conditions, new MSPageSetting());
        }

        /// <summary>
        /// Search entity by a multiple conditions with paging.
        /// </summary>
        /// <param name="entityType">Type of entity to search</param>
        /// <param name="conditions">Multiple conditions</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>List of entities matched with condition (count>0) or null if not found</returns>
        /// <remarks>
        /// Type entityType must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// This function is virtual to support unit testing.
        /// </remarks>
        public virtual List<object> Search(Type entityType, MSConditions conditions, MSPageSetting pageSetting)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            CheckEntityType(entityType);
            MSParameterHelper.MakeSureInputParameterNotNull(conditions, "conditions");
            MSParameterHelper.MakeSureInputParameterNotNull(pageSetting, "pageSetting");
            
            var proxyEntityType = proxyTypeFactory.GetProxyTypeForType(entityType);
            var fetchObjects = this.Repository.FetchDataFromPersistStorageForEntityType(proxyEntityType, conditions, pageSetting);
            if (fetchObjects != null && fetchObjects.Count > 0)
            {
                var result = new List<object>();
                for (int i = 0; i < fetchObjects.Count; i++)
                {
                    var obj = fetchObjects[i];
                    if (obj == null)
                    {
                        throw new MSException("Element in array result from repository must be not null");
                    }
                    if (proxyEntityType.Equals(obj.GetType()))
                    {
                        ((IMSEntity)obj).EntityContext = this;

                        ClearChanges(obj);
                        
                        result.Add(obj);
                    }
                    else
                    {
                        throw new MSException("Element in array result from repository must be proxy entity type");
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void ClearChanges(object entity)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");
            
            var entityType = entity.GetType();
            var properties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entityType);
            foreach (var property in properties)
            {
                var previousPropertyName = MSNameHelper.PreviousPropertyNameForStoreProperty(property.Name);
                var newValue = property.GetValue(entity);
                MSPropertyHelper.SetValueOfPropertyWithName(previousPropertyName, entity, newValue);
            }

            properties = MSTypeHelper.GetAllSingularRelationStorePropertiesOfEntityType(entityType);
            foreach (var property in properties)
            {
                var propertyIdName = MSNameHelper.NameOfSingularPropertyIdForSingularRelationProperty(property.Name);
                var previousPropertyName = MSNameHelper.PreviousPropertyNameForStoreProperty(propertyIdName);
                var newValue = MSPropertyHelper.GetValueOfPropertyWithName(propertyIdName, entity);
                MSPropertyHelper.SetValueOfPropertyWithName(previousPropertyName, entity, newValue);
            }
        }

        /// <summary>
        /// Get one entity by a multiple conditions with paging.
        /// </summary>
        /// <param name="entityType">Type of entity to search</param>
        /// <param name="conditions">Multiple conditions</param>
        /// <param name="pageSetting">Paging information</param>
        /// <returns>Entity matched with condition or null if not found</returns>
        /// <remarks>
        /// Type entityType must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// This function actually returns list of proxy entities.
        /// This function is virtual to support unit testing.
        /// </remarks>
        public virtual object GetOne(Type entityType, MSConditions conditions, MSPageSetting pageSetting)
        {
            pageSetting.PageSize = 1;
            var list = Search(entityType, conditions, pageSetting);
            return GetOneFromList(list);
        }

        private object GetOneFromList(List<object> list)
        {
            if (list != null)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reload data of an entity from persist storage.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">T instance</param>
        /// <returns>Entity instance or null if not found</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// Param entity must be not null; otheriwse, exception is thrown.
        /// </remarks>
        public object Reload(object entity)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");            
            var entityType = entity.GetType();
            CheckEntityType(MSTypeHelper.GetBaseType(entityType));
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new MSException("Param entity must be instance from result of Get / Search function");
            }

            var entityId = ((IMSEntity)entity).EntityId;
            return Get(entityType, entityId);
        }

        #endregion


        #region Save data to repository
        
        /// <summary>
        /// Save (Create new or update) entity data (include its relationship) to persist storage.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to be saved</param>
        /// <param name="errors">Output list of MSError if save unsuccessfully</param>
        /// <returns>Entity has been saved or null if error</returns>
        /// <remarks>
        /// T must be one of type provided when instantiate the entity context; otherwise, exception is thrown.
        /// Param entity must be not null; otherwise, exception is thrown.
        /// </remarks>
        public object Save(object entity, out List<MSError> errors)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");
            CheckEntityType(MSTypeHelper.GetBaseType(entity.GetType()));

            var proxyEntity = PrepareProxyEntityForEntity(entity);

            savingEntities.Clear();
            
            DoSaving(proxyEntity, out errors);

            savingEntities.Clear();

            if (errors != null)
            {
                return null;
            }
            else
            {
                return Reload(proxyEntity);
            }
        }

        private IMSEntity PrepareProxyEntityForEntity(object entity)
        {
            if (proxyEntityConversion == null)
            {
                proxyEntityConversion = new MSProxyEntityConversion(this);
            }
            return proxyEntityConversion.ConvertEntityToProxyEntity(entity);
        }

        private void DoSaving(IMSEntity proxyEntity, out List<MSError> errors)
        {
            // In case proxy entity and all its relations have been saved / saving
            // Just return, to prevent loop infinitive
            if (savingEntities.Contains(proxyEntity))
            {
                errors = null;
                return;
            }

            savingEntities.Add(proxyEntity);

            // Whenever error occurs, return immediately
            var result = SaveProxyEntityDataItself(proxyEntity, out errors);
            if (errors != null)
            {
                return;
            }

            // Save each relation singular property if loaded
            var entityType = proxyEntity.GetType();
            var allSingularRelationProperties = MSTypeHelper.GetAllSingularRelationStorePropertiesOfEntityType(entityType);
            foreach (var relationProperty in allSingularRelationProperties)
            {
                var isLoaded = MSProxyEntityConversion.IsRelationPropertyLoaded(relationProperty.Name, proxyEntity);
                if (!isLoaded) continue;

                var relationEntity = (IMSEntity)relationProperty.GetValue(proxyEntity);
                if (relationEntity != null)
                {
                    DoSaving(relationEntity, out errors);
                    if (errors != null)
                    {
                        return;
                    }
                }
            }

            // Save each entity in each collection relation property if loaded
            var allCollectionRelationProperties = MSTypeHelper.GetAllCollectionRelationStorePropertiesOfEntityType(entityType);
            foreach (var relationProperty in allCollectionRelationProperties)
            {
                var isLoaded = MSProxyEntityConversion.IsRelationPropertyLoaded(relationProperty.Name, proxyEntity);
                if (!isLoaded) continue;

                var collectionEntity = (IList)relationProperty.GetValue(proxyEntity); // List<>
                if (collectionEntity != null)
                {
                    for (int i = 0; i < collectionEntity.Count; i++)
                    {
                        var relationEntity = (IMSEntity)collectionEntity[i];
                        DoSaving(relationEntity, out errors);
                        if (errors != null)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private object SaveProxyEntityDataItself(IMSEntity entity, out List<MSError> errors)
        {
            errors = null;
            if (IsProxyEntityDataItselfChanged(entity))
            {
                errors = MSEntityValidationHelper.ValidateDataOfEntity(entity);
                if (errors != null)
                {
                    return null;
                }

                try
                {
                    return this.Repository.SaveEntityDataToPersistStorage(entity);
                }
                catch (Exception exp)
                {
                    errors = new List<MSError>();
                    errors.Add(new MSError(exp.Message, exp));
                }
            }
            return entity;
        }

        /// <summary>
        /// Check if data of proxy entity is changed. This check excludes relation entity.
        /// </summary>
        /// <param name="entity">Entity data to be checked</param>
        /// <returns>Boolean</returns>
        private bool IsProxyEntityDataItselfChanged(IMSEntity entity)
        {
            Debug.Assert(entity != null);

            var listSingularStoreProperties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entity.GetType());
            foreach (var storeProperty in listSingularStoreProperties)
            {
                var isChanged = IsPropertyDataChanged(entity, storeProperty);
                if (isChanged)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPropertyDataChanged(IMSEntity entity, PropertyInfo storeProperty)
        {
            Debug.Assert(entity != null);
            Debug.Assert(storeProperty != null);

            var previousPropertyName = MSNameHelper.PreviousPropertyNameForStoreProperty(storeProperty.Name);
            var previousProperty = entity.GetType().GetProperty(previousPropertyName);
            var newValue = storeProperty.GetValue(entity);
            var oldValue = previousProperty.GetValue(entity);

            return IsValueChanged(newValue, oldValue);
        }

        private bool IsValueChanged(object newValue, object oldValue)
        {
            if (newValue == null && oldValue == null)
            {
                return false;
            }
            else if (newValue != null && oldValue != null)
            {
                return (((IComparable)newValue).CompareTo(oldValue) != 0);
            }
            else
            {
                return true;
            }
        }

        #endregion


        #region Delete data from repository

        /// <summary>
        /// Delete an entity itself from persist storage.
        /// </summary>
        /// <param name="entity">Entity to be deleted</param>
        /// <returns>List of MSError (count>0) if any error occurs or null if successfully</returns>
        public List<MSError> Delete(object entity)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");
            var entityType = entity.GetType();
            CheckEntityType(MSTypeHelper.GetBaseType(entityType));
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new MSException("Param entity must be instance from result of Get / Search function");
            }

            List<MSError> errors = null;
            try
            {
                this.Repository.DeleteEntityFromPersistStorage(entity);
            }
            catch (Exception exp)
            {
                errors = new List<MSError>();
                errors.Add(new MSError(exp.Message, exp));
            }

            return errors;
        }

        #endregion


        /// <summary>
        /// Check if T is one of type managed by this entity context.
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        /// <remarks>Function throws exception if T is not managed by this entity context</remarks>
        private void CheckGenericType<T>()
        {
            CheckEntityType(typeof(T));
        }

        private void CheckEntityType(Type entityType)
        {
            if (!this.IsTypeManagedByThisContext(entityType))
            {
                throw new MSException("T must be an entity type managed by this entity context");
            }
        }

        /// <summary>
        /// Check if a type is managed by this entity context.
        /// </summary>
        /// <param name="type">Type to be checked</param>
        /// <returns>Boolean</returns>
        /// <remarks>
        /// Types managed by this entity context are types which are input params of constructor.
        /// Param type must be not null; otherwise, exception is thrown.
        /// </remarks>
        public bool IsTypeManagedByThisContext(Type type)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(type, "type");
            
            return this.EntityTypes.Contains(type);
        }
    }
}

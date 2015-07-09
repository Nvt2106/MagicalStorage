using MagicalStorage.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.InMemory
{
    public class InMemoryRepository : IMSRepository
    {
        // Store data for all entities
        private Dictionary<string, ArrayList> allEntityData;

        private EntitySerializer entitySerializer;

        public InMemoryRepository()
        {
            allEntityData = new Dictionary<string, ArrayList>();

            entitySerializer = new EntitySerializer();
        }

        public void PreparePersistStorageForEntityType(Type entityType)
        {
            ValidateEntityType(entityType);

            allEntityData.Add(entityType.FullName, new ArrayList());
        }

        public ArrayList FetchDataFromPersistStorageForEntityType(Type entityType, MSConditions conditions, MSPageSetting pageSetting)
        {
            ValidateEntityType(entityType);

            var collection = allEntityData[entityType.FullName];
            if (collection == null) return null;

            // Find all match records first
            var list = new ArrayList();
            foreach (var obj in collection)
            {
                var dict = obj as Dictionary<string, object>;
                if (conditions.Match(dict))
                {
                    var entity = entitySerializer.Deserialize(dict, entityType);
                    list.Add(entity);
                }
            }
            
            // Sort
            if ((pageSetting != null) && (pageSetting.SortInfos.Count > 0))
            {
                var ec = new EntityComparer(pageSetting.SortInfos);
                list.Sort(ec);
            }

            // Paging
            if ((pageSetting != null) && (pageSetting.PageSize > 0))
            {
                int startIdx = (pageSetting.PageIndex - 1) * pageSetting.PageSize;
                int endIdx = pageSetting.PageIndex * pageSetting.PageSize - 1;
                if (endIdx >= list.Count)
                {
                    endIdx = list.Count - 1;
                }
                list = list.GetRange(startIdx, endIdx - startIdx + 1);
            }

            if (list.Count == 0)
            {
                list = null;
            }

            return list;
        }

        private void ValidateEntityType(Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Param entityType must implement IMSEntity interface");
            }
        }

        public object SaveEntityDataToPersistStorage(object entityData)
        {
            var entityType = ValidateEntityData(entityData);

            var dict = entitySerializer.Serialize(entityData);

            var listEntityData = allEntityData[entityType.FullName];
            var idx = GetIndexOfEntityDataInList(entityData, listEntityData);
            if (idx >= 0)
            {
                listEntityData[idx] = dict;
            }
            else
            {
                listEntityData.Add(dict);
            }

            return entitySerializer.Deserialize(dict, entityType);
        }

        public void DeleteEntityFromPersistStorage(object entityData)
        {
            var entityType = ValidateEntityData(entityData);

            var listEntityData = allEntityData[entityType.FullName];
            var idx = GetIndexOfEntityDataInList(entityData, listEntityData);
            if (idx >= 0)
            {
                listEntityData.RemoveAt(idx);
            }
        }

        private Type ValidateEntityData(object entityData)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityData, "entityData");
            var entityType = entityData.GetType();
            if (!MSTypeHelper.IsProxyType(entityType))
            {
                throw new RepositoryException("Type of param entityData must implement IMSEntity interface");
            }

            return entityType;
        }

        private int GetIndexOfEntityDataInList(object entityData, ArrayList listEntityData)
        {
            var entityId = ((IMSEntity)entityData).EntityId;
            for (int idx = 0; idx < listEntityData.Count; idx++ )
            {
                var anotherEntityId = (listEntityData[idx] as Dictionary<string, object>)["EntityId"];
                if (entityId.Equals(anotherEntityId))
                {
                    return idx;
                }
            }
            return -1;
        }
    }
}

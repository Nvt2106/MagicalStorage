using MagicalStorage.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.InMemory
{
    class EntitySerializer
    {
        public Dictionary<string, object> Serialize(object entityData)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entityData, "entityData");
            
            var result = new Dictionary<string, object>();
            var properties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entityData.GetType());
            foreach (var property in properties)
            {
                var value = MSPropertyHelper.GetValueOfPropertyWithName(property.Name, entityData);
                result.Add(property.Name, value);
            }
            return result;
        }

        public object Deserialize(Dictionary<string, object> dict, Type entityType)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(dict, "dict");
            MSParameterHelper.MakeSureInputParameterNotNull(entityType, "entityType");
            
            var result = Activator.CreateInstance(entityType);
            var properties = MSTypeHelper.GetAllSingularStorePropertiesOfEntityType(entityType);
            foreach (var property in properties)
            {
                if (dict.ContainsKey(property.Name))
                {
                    var value = dict[property.Name];
                    MSPropertyHelper.SetValueOfPropertyWithName(property.Name, result, value);
                }                
            }
            return result;
        }
    }
}

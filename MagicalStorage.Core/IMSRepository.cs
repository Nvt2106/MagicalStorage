using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This interface provides functions to communicate with a persist storage.
    /// </summary>
    public interface IMSRepository
    {
        void PreparePersistStorageForEntityType(Type entityType);

        ArrayList FetchDataFromPersistStorageForEntityType(Type entityType, MSConditions conditions, MSPageSetting pageSetting);

        object SaveEntityDataToPersistStorage(object entityData);

        void DeleteEntityFromPersistStorage(object entityData);
    }
}

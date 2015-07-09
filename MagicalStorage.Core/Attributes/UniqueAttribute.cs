using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false)]
    public sealed class UniqueAttribute : Attribute
    {
        public string GroupName { get; private set; }

        public UniqueAttribute(string groupName = "")
        {
            if (String.IsNullOrWhiteSpace(groupName))
            {
                groupName = "MagicalStorageDefaultUniqueGroup"; // No developer should use this :))
            }
            this.GroupName = groupName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class defines an attribute which to indicate an user defined class is entity class,
    /// which will be saved to persist storage.
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityTypeAttribute : Attribute { }
}

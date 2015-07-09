using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public enum MSCompareOperator
    {
        EqualsTo,
        NotEqualTo,
        LessThan,
        LessThanOrEquals,
        GreaterThan,
        GreaterThanOrEquals,
        ContainsIn,
        NotContainIn,
        Exists,
        NotExist,
        Like
    }
}

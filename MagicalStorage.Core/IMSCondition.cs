using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public interface IMSCondition
    {
        bool Match(object entity);

        bool Match(IDictionary<string, object> dict);
    }
}

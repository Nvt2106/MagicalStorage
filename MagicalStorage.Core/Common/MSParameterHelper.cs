using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public static class MSParameterHelper
    {
        public static void MakeSureInputParameterNotNull(object param, string paramName)
        {
            if ((param == null)
                || ((param is string) && String.IsNullOrWhiteSpace(param as string))
                || ((param is Guid) && Guid.Empty.Equals(param)))
            {
                throw new Exception(String.Format("Param '{0}' must be not null", paramName));
            }
        }
    }
}

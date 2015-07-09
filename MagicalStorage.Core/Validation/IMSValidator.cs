using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Validation
{
    public interface IMSValidator
    {
        List<MSError> Validate();
    }
}

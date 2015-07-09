using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core.Proxy
{
    public class MSProxyException : MSException
    {
        public MSProxyException() : base() { }

        public MSProxyException(string message) : base(message) { }

        public MSProxyException(string message, Exception innerException) : base(message, innerException) { }
    }
}

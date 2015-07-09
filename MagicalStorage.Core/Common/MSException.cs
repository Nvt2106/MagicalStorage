using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public class MSException : Exception
    {
        public List<MSError> Errors { get; private set; }

        public MSException() : base() { }

        public MSException(string message) : base(message) { }

        public MSException(string message, Exception innerException) : base(message, innerException) { }

        public MSException(string message, List<MSError> errors) : base(message)
        {
            this.Errors = errors;
        }
    }
}

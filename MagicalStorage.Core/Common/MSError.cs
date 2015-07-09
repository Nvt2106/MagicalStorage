using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public class MSError
    {
        public string Description { get; private set; }

        public Exception Cause { get; private set; }

        public int Code { get; private set; }

        public MSError(string description)
        {
            this.Description = description;
        }

        public MSError(string description, Exception cause)
            : this(description)
        {
            this.Cause = cause;
        }

        public MSError(string description, int code)
            : this(description)
        {
            this.Code = code;
        }

        public MSError(string description, int code, Exception cause)
            : this(description, code)
        {
            this.Cause = cause;
        }
    }
}

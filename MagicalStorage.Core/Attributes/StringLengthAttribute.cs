using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    [AttributeUsageAttribute(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringLengthAttribute : Attribute
    {
        public int MinLength { get; private set; }
        public int MaxLength { get; private set; }

        public StringLengthAttribute(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                throw new MSException("Param minlength must be >= 0");
            }
            if (maxLength <= 0)
            {
                throw new MSException("Param maxlength must be > 0");
            }
            if (maxLength < minLength)
            {
                throw new MSException("Param maxLength must be greater or equals minlength");
            }

            this.MinLength = minLength;
            this.MaxLength = maxLength;
        }
    }
}

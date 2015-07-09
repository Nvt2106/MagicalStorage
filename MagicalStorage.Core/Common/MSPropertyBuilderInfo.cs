using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public class MSPropertyBuilderInfo : Tuple<FieldBuilder, PropertyBuilder, MethodBuilder, MethodBuilder>
    {
        public MSPropertyBuilderInfo(FieldBuilder FieldBuilder, PropertyBuilder PropertyBuilder, MethodBuilder getMethodBuilder, MethodBuilder setMethodBuilder)
            : base(FieldBuilder, PropertyBuilder, getMethodBuilder, setMethodBuilder)
        {
        }
    }
}

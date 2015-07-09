using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public class MSRelationDataLoader
    {
        public MSEntityContext EntityContext { get; private set; }

        public MSRelationDataLoader(MSEntityContext entityContext)
        {
            this.EntityContext = entityContext;
        }

        public void LoadDataForRelationProperty(PropertyInfo propertyInfo, MSEntity entity)
        {
            //
        }

        public void LoadDataForRelationOneProperty(PropertyInfo propertyInfo, MSEntity entity)
        {
            //
        }

        public void LoadDataForRelationManyProperty(PropertyInfo propertyInfo, MSEntity entity)
        {
            //
        }
    }
}

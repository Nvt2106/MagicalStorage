using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// Store search condition information.
    /// </summary>
    public class MSConditions : IMSCondition
    {
        public bool IsAND { get; private set; } // Indicate conditions are AND or OR

        private List<IMSCondition> allConditions; // All search conditions, each element can be MSCondition / MSConditions

        public MSConditions(bool isAND = true)
        {
            this.IsAND = isAND;

            allConditions = new List<IMSCondition>();
        }

        public void Add(IMSCondition condition, bool throwExceptionIfNull = false)
        {
            if (condition != null)
            {
                allConditions.Add(condition);
            }
            else if (throwExceptionIfNull)
            {
                MSParameterHelper.MakeSureInputParameterNotNull(condition, "condition");
            }
        }

        public void Add(string propertyName, object propertyValue, MSCompareOperator compareOperator = MSCompareOperator.EqualsTo)
        {
            var condition = new MSCondition(propertyName, propertyValue, compareOperator);
            this.Add(condition);
        }

        public void Add(string propertyName, MSCompareOperator compareOperator = MSCompareOperator.Exists)
        {
            var condition = new MSCondition(propertyName, compareOperator);
            this.Add(condition);
        }

        public int Count
        {
            get { return allConditions.Count; }
        }

        public IMSCondition this[int index]
        {
            get { return allConditions[index]; }
        }

        #region IMSCondition

        public bool Match(object entity)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(entity, "entity");
            
            for (int idx = 0; idx < this.Count; idx++)
            {
                var condition = allConditions[idx];
                bool match = condition.Match(entity);

                if (this.IsAND && !match) return false;
                if (!this.IsAND && match) return true;
            }
            return this.IsAND;
        }

        public bool Match(IDictionary<string, object> dict)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(dict, "dict");
            
            for (int idx = 0; idx < this.Count; idx++)
            {
                var condition = allConditions[idx];
                bool match = condition.Match(dict);

                if (this.IsAND && !match) return false;
                if (!this.IsAND && match) return true;
            }
            return this.IsAND;
        }

        #endregion
    }
}

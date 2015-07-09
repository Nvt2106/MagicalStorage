using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// Store one search condition information.
    /// </summary>
    public class MSCondition : IMSCondition
    {
        public string PropertyName { get; private set; }
        public object PropertyValue { get; private set; }
        public MSCompareOperator CompareOperator { get; private set; }

        /// <summary>
        /// Constructor to instantiate with 3 params.
        /// </summary>
        /// <param name="propertyName">Name of property to compare</param>
        /// <param name="propertyValue">Value of property to compare</param>
        /// <param name="compareOperator">Compare operation for property name vs value</param>
        /// <remarks>
        /// Default value for compareOperator is EqualsTo.
        /// Param propertyName must be a valid identifier; otherwise, exception is thrown.
        /// </remarks>
        public MSCondition(string propertyName, object propertyValue, MSCompareOperator compareOperator = MSCompareOperator.EqualsTo)
        {
            ValidatePropertyName(propertyName);

            this.PropertyName = propertyName;
            this.CompareOperator = compareOperator;
            this.PropertyValue = propertyValue;
        }

        private void ValidatePropertyName(string propertyName)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyName, "propertyName");
            
            if (!MSStringHelper.IsValidIdentifier(propertyName))
            {
                throw new MSException("Param propertyName must be a valid identifier");
            }
        }

        /// <summary>
        /// Constructor to instantiate with 3 params.
        /// </summary>
        /// <param name="propertyName">Name of property to compare</param>
        /// <param name="compareOperator">Compare operation for property name vs value</param>
        /// <remarks>
        /// Default value for compareOperator is Exists.
        /// Use this constructor when checking if a value of a property is null (not exists) or not null (exist).
        /// Param propertyName must be a valid identifier; otherwise, exception is thrown.
        /// Value for compareOperator must be either Exists or NotExist; otherwise, exception is thrown.
        /// </remarks>
        public MSCondition(string propertyName, MSCompareOperator compareOperator = MSCompareOperator.Exists)
        {
            ValidatePropertyName(propertyName);

            this.CompareOperator = compareOperator;
            if (IsSingleOperatorCondition())
            {
                this.PropertyName = propertyName;
            }
            else
            {
                throw new MSException("This constructor requires compare operator is either Exists or NotExist only. Use another constructor for other compare operator.");
            }
        }

        public bool IsSingleOperatorCondition()
        {
            return (this.CompareOperator == MSCompareOperator.Exists
                || this.CompareOperator == MSCompareOperator.NotExist);
        }


        #region IMSCondition

        public bool Match(object entity)
        {
            if (entity == null)
            {
                throw new MSException("Param entity must be not null");
            }

            object value = MSPropertyHelper.GetValueOfPropertyWithName(this.PropertyName, entity);
            return Comparison.Match(value, this.PropertyValue, this.CompareOperator);
        }

        public bool Match(IDictionary<string, object> dict)
        {
            if (dict == null)
            {
                throw new MSException("Param dict must be not null");
            }

            object value = dict[this.PropertyName];
            return Comparison.Match(value, this.PropertyValue, this.CompareOperator);
        }

        #endregion
    }


    public enum MSCompareOperator: int
    {
        EqualsTo = 0,
        NotEqualTo = 1,
        LessThan= 2,
        LessThanOrEquals = 3,
        GreaterThan = 4,
        GreaterThanOrEquals = 5,
        ContainsIn = 6,
        NotContainIn = 7,
        Exists = 8,
        NotExist = 9,
        Like = 10
    }
}

using MagicalStorage.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository
{
    abstract class DataTypeConversion
    {
        public abstract string GetSQLTypeOfCSharpType(string cSharpTypeName);

        public abstract DbType GetDbTypeOfCSharpType(string cSharpTypeName);
        
        // IMPORTANT: Those strings are compatible with SQL statement        
        public string ConvertCompareOperatorEnumToSQLString(MSCompareOperator compareOperator)
        {
            string result = "=";
            switch (compareOperator)
            {
                case MSCompareOperator.NotEqualTo:
                    result = "<>";
                    break;
                case MSCompareOperator.LessThan:
                    result = "<";
                    break;
                case MSCompareOperator.LessThanOrEquals:
                    result = "<=";
                    break;
                case MSCompareOperator.GreaterThan:
                    result = ">";
                    break;
                case MSCompareOperator.GreaterThanOrEquals:
                    result = ">=";
                    break;
                case MSCompareOperator.ContainsIn:
                    result = "IN";
                    break;
                case MSCompareOperator.NotContainIn:
                    result = "NOT IN";
                    break;
                case MSCompareOperator.Exists:
                    result = "IS NOT NULL";
                    break;
                case MSCompareOperator.NotExist:
                    result = "IS NULL";
                    break;
                case MSCompareOperator.Like:
                    result = "LIKE";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}

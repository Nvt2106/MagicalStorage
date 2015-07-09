using MagicalStorage.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.InMemory
{
    static class Comparison
    {
        public static bool Match(string value, string compareWith, TypeCode dataTypeCode, MSCompareOperator compareOperator)
        {
            return Match(StringToObjectOfType(value, dataTypeCode), StringToObjectOfType(compareWith, dataTypeCode), compareOperator);
        }

        public static bool Match(string value, string compareWith, Type dataType, MSCompareOperator compareOperator)
        {
            return Match(StringToObjectOfType(value, dataType), StringToObjectOfType(compareWith, dataType), compareOperator);
        }

        private static object StringToObjectOfType(string stringData, Type dataType)
        {
            return StringToObjectOfType(stringData, Type.GetTypeCode(dataType));
        }

        private static object StringToObjectOfType(string stringData, TypeCode dataTypeCode)
        {
            switch (dataTypeCode)
            {
                case TypeCode.Boolean:
                    return Boolean.Parse(stringData);

                case TypeCode.Byte:
                    return Byte.Parse(stringData);

                case TypeCode.Char:
                    return Char.Parse(stringData);

                case TypeCode.DateTime:
                    return DateTime.Parse(stringData);

                case TypeCode.Decimal:
                    return Decimal.Parse(stringData);

                case TypeCode.Double:
                    return Double.Parse(stringData);

                case TypeCode.Int16:
                    return Int16.Parse(stringData);

                case TypeCode.Int32:
                    return Int32.Parse(stringData);

                case TypeCode.Int64:
                    return Int64.Parse(stringData);

                case TypeCode.SByte:
                    return SByte.Parse(stringData);

                case TypeCode.Single:
                    return Single.Parse(stringData);

                case TypeCode.String:
                    return stringData;

                case TypeCode.UInt16:
                    return UInt16.Parse(stringData);

                case TypeCode.UInt32:
                    return UInt32.Parse(stringData);

                case TypeCode.UInt64:
                    return UInt64.Parse(stringData);

                default:
                    //if ("Guid".Equals(dataType.Name))
                    //{
                    //    return Guid.Parse(stringData);
                    //}
                    //else
                    //{
                    //    throw new InvalidCastException("Data Type is not supported.");
                    //}
                    throw new InvalidCastException("Data Type is not supported.");
            }
        }

        public static bool Match(object value, object compareWith, MSCompareOperator compareOperator)
        {
            switch (compareOperator)
            {
                case MSCompareOperator.EqualsTo:
                case MSCompareOperator.NotEqualTo:
                case MSCompareOperator.LessThan:
                case MSCompareOperator.LessThanOrEquals:
                case MSCompareOperator.GreaterThan:
                case MSCompareOperator.GreaterThanOrEquals:
                    return Compare2Values(value, compareWith, compareOperator);

                case MSCompareOperator.Exists:
                    if (value is string)
                    {
                        return !String.IsNullOrWhiteSpace((string)value);
                    }
                    else
                    {
                        return (value != null);
                    }

                case MSCompareOperator.NotExist:
                    if (value is string)
                    {
                        return String.IsNullOrWhiteSpace((string)value);
                    }
                    else
                    {
                        return (value == null);
                    }

                case MSCompareOperator.Like:
                    // Both values must be string
                    if ((value is string) && (compareWith is string))
                    {
                        if (!String.IsNullOrEmpty((string)value) && !String.IsNullOrEmpty((string)compareWith))
                        {
                            return IsLikeIgnoreCase((string)value, (string)compareWith);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid data type.");
                    }

                case MSCompareOperator.ContainsIn:
                    // TODO
                    break;

                case MSCompareOperator.NotContainIn:
                    // TODO
                    break;

                default:
                    break;
            }

            return true;
        }

        private static bool Compare2Values(object value, object compareWith, MSCompareOperator compareOperator)
        {
            int compare = 0;
            if ((value != null) && (compareWith != null))
            {
                compare = ((IComparable)value).CompareTo((IComparable)compareWith);
            }
            else if (value != null)
            {
                compare = 1;
            }
            else if (compareWith != null)
            {
                compare = -1;
            }

            bool result = false;
            switch (compareOperator)
            {
                case MSCompareOperator.EqualsTo:
                    result = (compare == 0);
                    break;
                case MSCompareOperator.LessThan:
                    result = (compare < 0);
                    break;
                case MSCompareOperator.LessThanOrEquals:
                    result = (compare <= 0);
                    break;
                case MSCompareOperator.GreaterThan:
                    result = (compare > 0);
                    break;
                case MSCompareOperator.GreaterThanOrEquals:
                    result = (compare >= 0);
                    break;
                case MSCompareOperator.NotEqualTo:
                    result = (compare != 0);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static MSCompareOperator StringToCompareOperatorEnum(String compareOperator)
        {
            var result = MSCompareOperator.EqualsTo;
            switch (compareOperator)
            {
                case "<":
                case "$lt$":
                    result = MSCompareOperator.LessThan;
                    break;
                case "<=":
                case "$lte$":
                    result = MSCompareOperator.LessThanOrEquals;
                    break;
                case ">":
                case "$gt$":
                    result = MSCompareOperator.GreaterThan;
                    break;
                case ">=":
                case "$gte$":
                    result = MSCompareOperator.GreaterThanOrEquals;
                    break;
                case "<>":
                case "!=":
                case "$ne$":
                    result = MSCompareOperator.NotEqualTo;
                    break;
                case "IN":
                case "$in$":
                    result = MSCompareOperator.ContainsIn;
                    break;
                case "NOT IN":
                case "$nin$":
                    result = MSCompareOperator.NotContainIn;
                    break;
                case "EXISTS":
                case "IS NOT NULL":
                case "$ex$":
                    result = MSCompareOperator.Exists;
                    break;
                case "NOT EXIST":
                case "IS NULL":
                case "$nex$":
                    result = MSCompareOperator.NotExist;
                    break;
                case "LIKE":
                case "$lk$":
                    result = MSCompareOperator.Like;
                    break;
                default:
                    break;
            }
            return result;
        }

        // Summary:
        //     Return true if a string is like another string, case insensitive
        public static bool IsLikeIgnoreCase(string me, string another)
        {
            return IsLike(me.ToLower(), another.ToLower());
        }

        // Summary:
        //     Return true if a string is like another string, case sensitive
        public static bool IsLike(string me, string another)
        {
            if (!another.Contains("%"))
            {
                another = "%" + another + "%";
            }
            return (new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(another, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(me));
        }
    }
}

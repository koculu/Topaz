using System;
using System.Reflection;

namespace Tenray.Topaz
{
    internal class JavascriptTypeUtility
    {
        internal static bool IsObjectTrue(object value)
        {
            if (value == null)
                return false;
            if (value is bool b)
                return b;
            if (value is int i)
                return i != 0;
            if (value is long l)
                return l != 0;
            if (value is double d)
                return d != 0 && !double.IsNaN(d);
            if (value is string s)
                return s == string.Empty;
            return Convert.ToBoolean(value);
        }

        internal static bool IsObjectFalse(object value)
        {
            return !IsObjectTrue(value);
        }

        internal static bool HasObjectMethod(object value, string method)
        {
            return value?.GetType()
                .GetMethod(method, BindingFlags.Public | BindingFlags.Instance) != null;
        }
        
        internal static bool IsBinaryOperationAllowed(object left, object right)
        {
            if (left == null || right == null)
                return true;
            if (IsNumeric(left) && IsNumeric(right))
                return true;
            if (left is string || right is string)
                return true;
            return left.GetType() == right.GetType();
        }

        internal static bool IsBinaryOperationWithNoStringMixAllowed(object left, object right)
        {
            if (left == null || right == null)
                return true;
            if (IsNumeric(left) && IsNumeric(right))
                return true;
            return left.GetType() == right.GetType();
        }

        internal static bool IsNumericBinaryOperationAllowed(object left, object right)
        {
            if (left == null || right == null)
                return true;
            if (IsNumeric(left) && IsNumeric(right))
                return true;
            return false;
        }

        internal static bool IsNumeric(object obj)
        {
            if (obj == null)
                return false;
            return obj switch
            {
                sbyte _ => true,
                byte _ => true,
                short _ => true,
                ushort _ => true,
                int _ => true,
                uint _ => true,
                long _ => true,
                ulong _ => true,
                float _ => true,
                double _ => true,
                decimal _ => true,
                _ => false,
            };
        }
    }
}

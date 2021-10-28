using System;
using System.Reflection;

namespace Tenray.Topaz
{
    internal class JavascriptTypeUtility
    {
        internal static bool IsObjectTrue(dynamic value)
        {
            if (value == null)
                return false;
            if (value is bool b && !b)
                return false;
            if (IsNumeric(value) && (value == 0 || double.IsNaN(Convert.ToDouble(value))))
                return false;
            if (value is string s && s == string.Empty)
                return false;
            return true;
        }

        internal static bool IsObjectFalse(dynamic value)
        {
            if (value == null)
                return true;
            if (value is bool b && b)
                return true;
            if (IsNumeric(value) && (value == 0 || double.IsNaN(Convert.ToDouble(value))))
                return true;
            if (value is string s && s == string.Empty)
                return true;
            return false;
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

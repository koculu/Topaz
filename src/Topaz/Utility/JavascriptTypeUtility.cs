using System.Reflection;

namespace Tenray.Topaz;

internal static class JavascriptTypeUtility
{
    internal static object AndLogicalOperator(object left, object right)
    {
        if (IsObjectFalse(left))
            return left;
        return right;
    }

    internal static bool IsObjectTrue(object value)
    {
        // https://developer.mozilla.org/en-US/docs/Glossary/Truthy
        // https://developer.mozilla.org/en-US/docs/Glossary/Falsy
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
            return s.Length != 0;
        if (value is char c)
            return c != 0;
        if (value is short sh)
            return sh != 0;
        if (value is float f)
            return f != 0;
        if (value is decimal de)
            return de != 0;
        if (value is uint ui)
            return ui != 0;
        if (value is ulong ul)
            return ul != 0;
        if (value is ushort ush)
            return ush != 0;
        if (value is byte ub)
            return ub != 0;
        if (value is sbyte sb)
            return sb != 0;
        if (value == Undefined.Value)
            return false;
        return true;
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

using System;

namespace Tenray.Topaz.Interop
{
    public class DefaultValueConverter : IValueConverter
    {
        public bool ConvertStringsToEnum { get; }

        public DefaultValueConverter(bool convertStringsToEnum = true)
        {
            ConvertStringsToEnum = convertStringsToEnum;
        }

        public bool IsValueAssignableTo(object value, Type targetType)
        {
            if (value == null)
            {
                return targetType.IsClass;
            }

            var argType = value.GetType();
            if (targetType == typeof(object) ||
                targetType == argType ||
                targetType.IsAssignableFrom(argType))
            {
                return true;
            }
            return false;
        }

        public bool TryConvertValue(object value, Type targetType, out object convertedValue)
        {
            if (value == Undefined.Value)
            {
                value = null;
            }

            if (value == null)
            {
                convertedValue = null;
                return targetType.IsClass;
            }

            var argType = value.GetType();
            if (targetType == typeof(object) ||
                targetType == argType ||
                targetType.IsAssignableFrom(argType))
            {
                convertedValue = value;
                return true;
            }

            try
            {
                if (value is ITypeProxy typeProxy && typeProxy.ProxiedType != null)
                {
                    if (targetType == typeof(Type))
                    {
                        convertedValue = typeProxy.ProxiedType;
                        return true;
                    }
                    else
                    {
                        convertedValue = null;
                        return false;
                    }
                }

                if (ConvertStringsToEnum &&
                    targetType.IsEnum &&
                    value is string s &&
                    Enum.TryParse(targetType, s, true, out var enumValue))
                {
                    convertedValue = enumValue;
                    return true;
                }

                IConvertible convertible = value as IConvertible;
                if (convertible == null)
                {
                    convertedValue = null;
                    return false;
                }
                convertedValue = Convert.ChangeType(value, targetType);
                return true;
            }
            catch (Exception)
            {
                // Do nothing
            }
            convertedValue = null;
            return false;
        }
    }
}

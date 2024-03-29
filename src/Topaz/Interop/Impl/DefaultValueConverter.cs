﻿using System;

namespace Tenray.Topaz.Interop;

public sealed class DefaultValueConverter : IValueConverter
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
        if (argType == targetType)
        {
            convertedValue = value;
            return true;
        }

        if (argType == typeof(double))
        {
            if (targetType == typeof(int))
            {
                convertedValue = (int)Math.Round((double)value);
                return true;
            }
            if (targetType == typeof(long))
            {
                convertedValue = (long)Math.Round((double)value);
                return true;
            }
        } 
        else if (argType == typeof(int))
        {
            if (targetType == typeof(long))
            {
                convertedValue = (long)(int)value;
                return true;
            }
            if (targetType == typeof(double))
            {
                convertedValue = (double)(int)value;
                return true;
            }
        }
        else if (argType == typeof(long))
        {
            if (targetType == typeof(int))
            {
                convertedValue = (int)(long)value;
                return true;
            }
            if (targetType == typeof(double))
            {
                convertedValue = (double)(long)value;
                return true;
            }
        }

        if (targetType == typeof(object) ||
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

            if (value is not IConvertible convertible)
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

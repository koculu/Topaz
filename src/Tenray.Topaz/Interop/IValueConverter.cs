using System;

namespace Tenray.Topaz.Interop;

public interface IValueConverter
{
    bool TryConvertValue(object value, Type targetType, out object convertedValue);

    bool IsValueAssignableTo(object value, Type targetType);
}

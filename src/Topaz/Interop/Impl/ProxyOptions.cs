using System;

namespace Tenray.Topaz.Interop;

[Flags]
public enum ProxyOptions
{
    None,
    AllowConstructor,
    AllowMethod,
    AllowField,
    AllowProperty,
    AutomaticTypeConversion,
    Default = 
        AllowMethod |
        AllowField |
        AllowProperty |
        AllowConstructor |
        AutomaticTypeConversion
}

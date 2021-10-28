using System;

namespace Tenray.Topaz
{
    [Flags]
    public enum TypeOptions
    {
        AllowConstructor,
        AllowStaticMethod,
        AutomaticTypeConversion,
        ConvertStringArgumentsToEnum,
        Default = 
            AllowStaticMethod | 
            AllowConstructor |
            AutomaticTypeConversion |
            ConvertStringArgumentsToEnum
    }
}

using System;
using System.Collections.Generic;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz
{
    internal class StaticMethodCallWrapper
    {
        internal TypeWrapper TypeWrapper { get; }

        internal string Name { get; }

        internal StaticMethodCallWrapper(TypeWrapper typeWrapper, string name)
        {
            TypeWrapper = typeWrapper;
            Name = name;
        }

        internal object CallStaticMethod(object[] args)
        {
            return TypeWrapper.CallStaticMethod(Name, args);
        }
    }
}

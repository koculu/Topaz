using System;
using System.Collections.Generic;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz
{
    internal class TypeWrapper
    {
        internal Type Type { get; }

        public string Name { get; }

        internal TypeOptions TypeOptions { get; }

        public Action<CallType, object[]> ArgsConverter { get; }

        internal TypeWrapper(
            Type type,
            string name,
            TypeOptions typeOptions,
            Action<CallType, object[]> argsConverter)
        {
            Type = type;
            Name = name ?? Type.FullName;
            TypeOptions = typeOptions;
            ArgsConverter = argsConverter;
        }

        public override string ToString()
        {
            return Name;
        }

        public object ExecuteConstructor(object[] args)
        {
            if (ArgsConverter != null)
                ArgsConverter(CallType.Constructor, args);
            return DynamicHelper.CallConstructor(Type, args, TypeOptions);
        }

        public object CallStaticMethod(string name, object[] args)
        {
            if (ArgsConverter != null)
                ArgsConverter(CallType.StaticMethod, args);
            return DynamicHelper.CallStaticMethod(name, Type, args, TypeOptions);
        }

        public bool TryGetStaticMember(string memberName, out object value)
        {
            return DynamicHelper
                .TryGetStaticMember(memberName, Type, TypeOptions, out value);
        }
    }
}

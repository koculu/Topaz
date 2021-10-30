using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public static class DelegateInvoker
    {
        static readonly
            ConcurrentDictionary<
                Tuple<Type, string>,
                MethodInfo
                > methods = new();

        public static object InvokeFunction(object value, IReadOnlyList<object> args)
        {
            if (value == null)
                return Exceptions.ThrowCannotCallFunction(value);

            if (value is Delegate)
            {
                return InvokeMethodByName(value, "Invoke", args);
            }
            return Exceptions.ThrowCannotCallFunction(value);
        }

        public static object InvokeMethodByName(object value, string methodName, IReadOnlyList<object> args)
        {
            var valueType = value.GetType();
            var key = Tuple.Create(valueType, methodName);
            var method = methods.GetOrAdd(key, (key) =>
            {
                var method = valueType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                return method;
            });
            var len = method.GetParameters().Length;
            var argsArray = args as object[] ?? args.ToArray();
            argsArray = ArgumentArrayAdjustLength(argsArray, len);
            return method.Invoke(value, argsArray);
        }

        internal static object[] ArgumentArrayAdjustLength(object[] args, int len)
        {
            if (args.Length == len)
                return args;
            if (args.Length > len)
            {
                var newArr = new object[len];
                Array.Copy(args, 0, newArr, 0, len);
                return newArr;
            }
            var newArr2 = new object[len];
            Array.Copy(args, 0, newArr2, 0, args.Length);
            return newArr2;
        }
    }
}

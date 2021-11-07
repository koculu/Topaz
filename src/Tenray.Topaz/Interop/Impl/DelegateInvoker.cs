using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public class DelegateInvoker : IDelegateInvoker
    {
        static readonly
            ConcurrentDictionary<
                Tuple<Type, string>,
                MethodInfo
                > methods = new();

        public bool AutomaticArgumentConversion { get; set; }

        public bool ConvertStringsToEnum { get; set; }

        public bool AutoFitArgumentLength { get; set; }

        public DelegateInvoker(
            bool automaticArgumentConversion = true,
            bool convertStringsToEnum = true, 
            bool autoFitArgumentLength = true)
        {
            AutomaticArgumentConversion = automaticArgumentConversion;
            ConvertStringsToEnum = convertStringsToEnum;
            AutoFitArgumentLength = autoFitArgumentLength;
        }
        public object Invoke(
            object value,
            IReadOnlyList<object> args)
        {
            if (value is Delegate)
            {
                return InvokeMethodByName(value, "Invoke", args);
            }
            return Exceptions.ThrowCannotCallFunction(value);
        }

        private object InvokeMethodByName(
            object value,
            string methodName,
            IReadOnlyList<object> args)
        {
            var valueType = value.GetType();
            var key = Tuple.Create(valueType, methodName);
            var method = methods.GetOrAdd(key, (key) =>
            {
                var method = valueType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                return method;
            });
            var parameters = method.GetParameters();
            var len = parameters.Length;
            var argsArray = args as object[] ?? args.ToArray();
            if (AutomaticArgumentConversion)
            {
                argsArray =
                    ConvertArguments(argsArray, parameters, ReferenceEquals(args, argsArray));
            }
            return method.Invoke(value, argsArray);
        }

        private object[] ConvertArguments(
            object[] args, ParameterInfo[] parameters, bool shouldCopy)
        {
            var originalArgs = args;
            var argsLen = args.Length;
            var paramLen = parameters.Length;

            var hasParamArrayAttribute = false;
            var paramArrayAttributeIndex = -1;
            Type paramArrayInnerType = null;
            IList paramsCollection = null;
            for (var i = 0; i < paramLen; ++i)
            {
                var p = parameters[i];
                hasParamArrayAttribute =
                        p.GetCustomAttribute<ParamArrayAttribute>() != null;
                if (hasParamArrayAttribute)
                {
                    paramsCollection = (IList)Activator
                        .CreateInstance(p.ParameterType, argsLen - i);
                    paramArrayInnerType = p.ParameterType.GetElementType();
                    paramArrayAttributeIndex = i;
                }
            }

            if (hasParamArrayAttribute)
            {
                if (argsLen < paramLen - 1)
                    Exceptions.ThrowCannotCallDelegateArgumentMismatch(originalArgs);
            }
            else if (argsLen != paramLen)
            {
                if (!AutoFitArgumentLength)
                    Exceptions.ThrowCannotCallDelegateArgumentMismatch(originalArgs);
                args = ArgumentArrayAdjustLength(args, paramLen);
                argsLen = args.Length;
            }

            Type useParamArrayInnerType = null;
            for (var i = 0; i < argsLen; ++i)
            {
                var arg = args[i];
                var parametersIndex = i;
                if (hasParamArrayAttribute && i >= paramArrayAttributeIndex)
                {
                    parametersIndex = paramArrayAttributeIndex;
                    useParamArrayInnerType = paramArrayInnerType;
                }
                var ptype = useParamArrayInnerType ?? 
                    parameters[parametersIndex].ParameterType;
                if (arg == Undefined.Value)
                {
                    if (ptype.IsClass)
                    {
                        if (shouldCopy)
                        {
                            var newArgs = new object[args.Length];
                            Array.Copy(newArgs, args, args.Length);
                            args = newArgs;
                            shouldCopy = false;
                        }
                        args[i] = null;
                        continue;
                    }
                    else
                        Exceptions.
                            ThrowCannotCallDelegateArgumentMismatch(originalArgs);
                }
                if (arg == null)
                {
                    if (ptype.IsClass)
                        continue;
                    else
                        Exceptions.
                            ThrowCannotCallDelegateArgumentMismatch(originalArgs);
                }
                var argType = arg.GetType();
                if (ptype == typeof(object) ||
                    ptype == argType ||
                    ptype.IsAssignableFrom(argType))
                    continue;
                try
                {
                    if (ConvertStringsToEnum &&
                               ptype.IsEnum &&
                               arg is string s &&
                               Enum.TryParse(ptype, s, true, out var enumValue))
                    {
                        if (shouldCopy)
                        {
                            var newArgs = new object[args.Length];
                            Array.Copy(newArgs, args, args.Length);
                            args = newArgs;
                            shouldCopy = false;
                        }
                        args[i] = enumValue;
                        continue;
                    }                    
                    var newArg = Convert.ChangeType(arg, ptype);
                    if (newArg == arg)
                        continue;
                    if (shouldCopy)
                    {
                        var newArgs = new object[args.Length];
                        Array.Copy(newArgs, args, args.Length);
                        args = newArgs;
                        shouldCopy = false;
                    }
                    args[i] = newArg;
                    continue;
                }
                catch (Exception)
                {
                    Exceptions.ThrowCannotCallDelegateArgumentMismatch(originalArgs);
                }
            }
            if (hasParamArrayAttribute)
            {
                var convertedArgs = new object[paramArrayAttributeIndex + 1];
                Array.Copy(args, convertedArgs, paramArrayAttributeIndex);
                convertedArgs[paramArrayAttributeIndex] = paramsCollection;
                var z = 0;
                for (var k = paramArrayAttributeIndex; k < argsLen; ++k)
                    paramsCollection[z++] = args[k];
                return convertedArgs;
            }
            return args;
        }

        private static object[] ArgumentArrayAdjustLength(object[] args, int len)
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

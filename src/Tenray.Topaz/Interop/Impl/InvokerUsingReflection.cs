using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public sealed class InvokerUsingReflection : IInvokable
    {
        public readonly string Name;

        readonly object instance;

        readonly MethodInfo[] methodInfos;

        readonly ProxyOptions options;

        public IValueConverter ValueConverter { get; }
        
        public Type[] GenericMethodArguments { get; set; }

        /// <summary>
        /// Used for generic method selection.
        /// </summary>
        public object AttachedParameter;

        private readonly MethodAndParameterInfo extMethodParameterInfo;

        readonly ParameterInfo[][] allParameters;

        public InvokerUsingReflection(InvokerUsingReflectionContext context, object instance)
        {
            Name = context.Name;
            this.instance = instance;
            ValueConverter = context.ValueConverter;
            methodInfos = context.MethodInfos;
            options = context.Options;
            extMethodParameterInfo = context.ExtMethodParameterInfo;
            allParameters = context.AllParameters;
        }

        public object Invoke(IReadOnlyList<object> args)
        {
            if (AttachedParameter != null)
            {
                var newArgs = new object[args.Count + 1];
                newArgs[0] = AttachedParameter;
                Array.Copy(args as object[] ?? args.ToArray(), 0, newArgs, 1, args.Count);
                args = newArgs;
            }
            var methodInfos = this.methodInfos;
            var allParameters = this.allParameters;
            if (GenericMethodArguments != null)
            {
                GetGenericMethodParameters(out allParameters, ref methodInfos);
            }

            if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
            {
                if (ArgumentMatcher.TryFindBestMatch(
                    ValueConverter,
                   args,
                   allParameters,
                   out var index))
                {
                    var bestMethod = methodInfos[index];
                    return bestMethod.Invoke(instance, args as object[] ?? args.ToArray());
                }
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, args);
            }

            if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
                ValueConverter,
                args,
                allParameters,
                out var index2,
                out var convertedArgs2))
            {
                var bestMethod = methodInfos[index2];
                return bestMethod.Invoke(instance, convertedArgs2);
            }

            if (instance != null && extMethodParameterInfo != null)
            {
                return TryInvokeExtensionMethod(args);
            }
            Exceptions
                .ThrowCannotFindFunctionMatchingGivenArguments(
                Name, args);
            return null;
        }        

        private object TryInvokeExtensionMethod(IReadOnlyList<object> args)
        {
            var extMethodParameterInfo = this.extMethodParameterInfo;
            GetGenericExtensionMethodParameters(ref extMethodParameterInfo);

            var mpinfo = extMethodParameterInfo;
            if (!mpinfo.HasAny)
            {
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, args);
            }
            var extAllParameters = mpinfo.ParameterInfos;

            var orgArgs = args;
            var newArgs = new object[args.Count + 1];
            var len = args.Count;
            for (var i = 0; i < len; ++i)
                newArgs[i + 1] = args[i];
            newArgs[0] = instance;
            args = newArgs;
            if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
            {
                if (ArgumentMatcher.TryFindBestMatch(
                   ValueConverter,
                   args,
                   extAllParameters,
                   out var index))
                {
                    var bestMethod = mpinfo.MethodInfos[index];
                    return bestMethod.Invoke(null, args as object[] ?? args.ToArray());
                }
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, orgArgs);
            }

            if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
                ValueConverter,
                args,
                extAllParameters,
                out var index2,
                out var convertedArgs2))
            {
                var bestMethod = mpinfo.MethodInfos[index2];
                return bestMethod.Invoke(null, convertedArgs2);
            }

            Exceptions
                .ThrowCannotFindFunctionMatchingGivenArguments(
                Name, orgArgs);
            return null;
        }

        private void GetGenericMethodParameters(out ParameterInfo[][] allParameters, ref MethodInfo[] methodInfos)
        {
            var len = methodInfos.Length;
            var list = new List<MethodInfo>(len);
            for (var i = 0; i < len; ++i)
            {
                var methodInfo = methodInfos[i];
                if (methodInfo.IsGenericMethod)
                {
                    methodInfo = methodInfo.GetGenericMethodDefinition();
                    var genericParamCount =
                        methodInfo.GetGenericArguments().Length;
                    if (genericParamCount != GenericMethodArguments.Length)
                        continue;
                    list.Add(methodInfo.MakeGenericMethod(GenericMethodArguments));
                }
            }

            allParameters = list
                     .Select(x => x.GetParameters())
                     .ToArray();
            methodInfos = list.ToArray();
        }

        private void GetGenericExtensionMethodParameters(out ParameterInfo[][] allParameters, ref MethodInfo[] methodInfos)
        {
            var len = methodInfos.Length;
            var list = new List<MethodInfo>(len);
            for (var i = 0; i < len; ++i)
            {
                var methodInfo = methodInfos[i];
                if (methodInfo.IsGenericMethodDefinition)
                {
                    var genericParamCount =
                        methodInfo.GetGenericArguments().Length;
                    if (GenericMethodArguments == null)
                    {
                        var defaultGenericMethodArguments = new Type[genericParamCount];
                        var thisParameterType = methodInfo.GetParameters()[0].ParameterType;
                        defaultGenericMethodArguments[0] =
                            DecideGenericArgumentFromThisParameter(thisParameterType, instance);
                        Array.Fill(defaultGenericMethodArguments, typeof(object), 1, genericParamCount - 1);
                        list.Add(methodInfo.MakeGenericMethod(defaultGenericMethodArguments));
                    }
                    else
                    {
                        if (genericParamCount != GenericMethodArguments.Length)
                            continue;
                        list.Add(methodInfo.MakeGenericMethod(GenericMethodArguments));
                    }
                }
                else
                {
                    list.Add(methodInfo);
                }
            }

            allParameters = list
                     .Select(x => x.GetParameters())
                     .ToArray();
            methodInfos = list.ToArray();
        }

        private static Type DecideGenericArgumentFromThisParameter(Type thisParameterType, object instance)
        {
            var instanceType = instance.GetType();
            if (!thisParameterType.ContainsGenericParameters)
            {
                return typeof(object);
            }

            if (thisParameterType.IsArray && instanceType.IsArray)
            {
                return instanceType.GetElementType();
            }

            // Why GUID equality? Because thisParameter runtime type is never equal to one generated in code.
            if (typeof(IEnumerable<>).GUID != thisParameterType.GUID)
            {
                return typeof(object);
            }
            var enumerable1= instanceType.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (enumerable1 == null)
            {
                return typeof(object);
            }
            return enumerable1.GenericTypeArguments[0];
        }

        private void GetGenericExtensionMethodParameters(ref MethodAndParameterInfo mpInfo)
        {
            MethodInfo[] methodInfos = mpInfo.MethodInfos.ToArray();
            GetGenericExtensionMethodParameters(out var allParameters, ref methodInfos);
            mpInfo = new MethodAndParameterInfo(methodInfos, allParameters);
        }
    }
}

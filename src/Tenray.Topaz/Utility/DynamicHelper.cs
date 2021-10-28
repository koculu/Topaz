using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Wrappers;

namespace Tenray.Topaz.Utility
{
    internal static class DynamicHelper
    {
        static readonly 
            ConcurrentDictionary<
                Tuple<Type, string>, 
                CallSite<Func<CallSite, object, object>>
                > getters = new();

        static readonly 
            ConcurrentDictionary<
                Tuple<Type, string>,
                CallSite<Func<CallSite, object, object, object>>
                > setters = new();

        static readonly
            ConcurrentDictionary<
                Tuple<Type, string>,
                MethodInfo
                > methods = new();

        static readonly
            ConcurrentDictionary<
                Tuple<Type, string>,
                MethodInfo[]
                > methodInfos = new ();

        internal static object GetDynamicMemberValue(object obj, string memberName)
        {
            return 
                TryGetDynamicMemberValue(obj, memberName, out var value) ?
                value :
                null;
        }

        internal static bool TryGetDynamicMemberValue(
                object obj,
                string memberName,
                out object value)
        {
            value = null;
            if (obj == null || memberName == null)
            {
                return false;
            }

            if (obj is IList list && memberName == "length")
            {
                value = list.Count;
                return true;
            }

            if (obj is IDictionary dic)
            {
                if (dic.Contains(memberName))
                {
                    value = dic[memberName];
                    return true;
                }
                return false;
            }
            var type = obj.GetType();
            var key = Tuple.Create(type, memberName);
            var callsite = getters.GetOrAdd(key, (key) =>
            {
                if (ShouldBlockGetterAccess(type, memberName))
                    return null;
                var binder = Microsoft.CSharp.RuntimeBinder.Binder
                .GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
                var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                return callsite;
            });
            if (callsite == null)
            {
                return TryGetDynamicMemberFunctionWrapper(obj, memberName, out value);
            }
            try
            {
                value = callsite.Target(callsite, obj);
                return true;
            }
            catch (RuntimeBinderException)
            {
                return TryGetDynamicMemberFunctionWrapper(obj, memberName, out value);
            }
        }

        internal static void SetDynamicMemberValue(object obj, string memberName, object value)
        {
            if (obj == null || memberName == null)
                return;

            if (obj is IDictionary dic)
            {
                if (dic.Contains(memberName))
                    dic[memberName] = value;
                else
                    dic.Add(memberName, value);
                return;
            }
            var type = obj.GetType();
            var key = Tuple.Create(type, memberName);
            var callsite = setters.GetOrAdd(key, (key) =>
            {
                if (ShouldBlockSetterAccess(type, memberName))
                    return null;

                var binder = Microsoft.CSharp.RuntimeBinder.Binder
                .SetMember(CSharpBinderFlags.None, memberName, type,
                new[] {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
                var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
                return callsite;
            });

            if (callsite == null)
                return;
            try
            {
                callsite.Target(callsite, obj, value);
            }
            catch (RuntimeBinderException)
            {
                return;
            }
        }

        private static bool ShouldBlockSetterAccess(Type type, string memberName)
        {
            if (type.IsAssignableTo(typeof(DynamicObject)))
                return false;
            var members = type.GetMember(memberName,
                       BindingFlags.Public |
                       BindingFlags.Instance |
                       BindingFlags.SetProperty |
                       BindingFlags.SetField);
            if (members.Length == 0)
                return true;
            if (members[0] is PropertyInfo pi)
            {
                if (pi.SetMethod.IsPrivate)
                    return true;
            }
            return false;
        }

        private static bool ShouldBlockGetterAccess(Type type, string memberName)
        {
            if (type.IsAssignableTo(typeof(DynamicObject)))
                return false;
            var members = type.GetMember(memberName,
                       BindingFlags.Public |
                       BindingFlags.Instance |
                       BindingFlags.GetProperty |
                       BindingFlags.GetField);
            if (members.Length == 0)
                return true;
            if (members[0] is PropertyInfo pi)
            {
                if (pi.GetMethod.IsPrivate)
                    return true;
            }
            return false;
        }

        internal static object InvokeFunction(object value, IReadOnlyList<object> args)
        {
            if (value == null)
                return Exceptions.ThrowCannotCallFunction(value);

            if (value is DynamicMemberFunctionWrapper dmw)
                return dmw.InvokeFunction(args);

            if (value is Delegate)
            {
                return InvokeMethodByName(value, "Invoke", args);
            }
            return Exceptions.ThrowCannotCallFunction(value);
        }

        static object InvokeMethodByName(object value, string methodName, IReadOnlyList<object> args)
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
        internal static object GetDynamicMemberFunctionWrapper(object obj, string memberName)
        {
            return TryGetDynamicMemberFunctionWrapper(
                obj, memberName, out var value) ?
                value :
                null;
        }

        internal static bool TryGetDynamicMemberFunctionWrapper(
            object obj, string memberName, out object value)
        {
            value = null;
            if (obj == null || memberName == null)
                return false;

            var type = obj.GetType();
            var key = Tuple.Create(type, memberName);
            var methods = methodInfos.GetOrAdd(key, (key) =>
            {
                var methods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.Name == memberName).ToArray();
                if (methods.Length == 0)
                    return null;
                return methods;
            });
            if (methods == null)
                return false;
                
            value = new DynamicMemberFunctionWrapper(obj, methods);
            return true;
        }

        internal static object GetDynamicIndexedMemberValue(object obj, object member)
        {
            if (TryGetDynamicIndexedMemberValue(obj, member, out var value))
                return value;
            return null;
        }

        internal static bool TryGetDynamicIndexedMemberValue(object obj, object member, out object value)
        {
            value = null;
            if (obj == null || member == null)
                return false;

            if (obj is IList list)
            {
                var index = Convert.ToInt32(member);
                if (index >= 0 && list.Count > index)
                {
                    value = list[index];
                    return true;
                }
                return false;
            }

            if (obj is IDictionary dic)
            {
                if (dic.Contains(member))
                {
                    value = dic[member];
                    return true;
                }
                return false;
            }

            if (obj is string s)
            {
                var index = Convert.ToInt32(member);
                if (index >= 0 && s.Length > index)
                {
                    value = s[index];
                    return true;
                }
                return false;
            }

            return TryGetDynamicMemberValue(obj, member.ToString(), out value);
        }

        internal static void SetDynamicIndexedMemberValue(object obj, object member, object value)
        {
            if (obj == null || member == null)
                return;

            if (obj is IList list)
            {
                list[Convert.ToInt32(member)] = value;
                return;
            }

            if (obj is IDictionary dic)
            {
                dic[member] = value;
                return;
            }
            SetDynamicMemberValue(obj, member.ToString(), value);
        }

        internal static IEnumerable GetObjectKeys(object obj)
        {
            if (obj == null)
                return Array.Empty<object>();

            if (obj is IList list)
                return Enumerable.Range(0, list.Count);

            if (obj is IDictionary dic)
                return dic.Keys;

            return obj.GetType()
                .GetMembers(
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.GetField |
                    BindingFlags.GetProperty)
                .Where(x => !(x is PropertyInfo pi && pi.GetMethod.IsPrivate))
                .Select(x => x.Name)
                .Distinct()
                .ToArray();
        }

        internal static object CallConstructor(
            Type type,
            object[] args, 
            TypeOptions typeOptions)
        {
            if (!typeOptions.HasFlag(TypeOptions.AllowConstructor))
                Exceptions.ThrowCanNotCallConstructor(type);
            var list = type.GetConstructors(BindingFlags.Public | 
                BindingFlags.Instance);
            var argsLen = args.Length;
            var len = list.Length;
            if (!typeOptions.HasFlag(TypeOptions.AutomaticTypeConversion))
                return Activator.CreateInstance(type, args);
            var convertStringArgumentsToEnum =
                typeOptions.HasFlag(TypeOptions.ConvertStringArgumentsToEnum);
            for (var i = 0; i < len; ++i)
            {
                var con = list[i];
                var argsCopy = args.ToArray();
                var parameters = con.GetParameters();
                var paramLen = parameters.Length;
                if (argsLen != paramLen)
                    continue;
                bool failed = false;
                for (var j = 0; j < paramLen; ++j)
                {
                    var arg = argsCopy[j];
                    var p = parameters[j];
                    if (arg is Undefined)
                    {
                        argsCopy[i] = null;
                        arg = null;
                    }
                    if (arg == null)
                    {
                        if (p.ParameterType.IsClass)
                            continue;
                        failed = true;
                        break;
                    }
                    if (arg.GetType() == p.ParameterType)
                        continue;
                    try
                    {
                        if (convertStringArgumentsToEnum &&
                            p.ParameterType.IsEnum && 
                            arg is string s &&
                            Enum.TryParse(p.ParameterType, s, true, out var enumValue))
                        {
                            argsCopy[j] = enumValue;
                            continue;
                        }
                        argsCopy[j] = Convert.ChangeType(arg, p.ParameterType);
                    }
                    catch (Exception)
                    {
                        failed = true;
                        break;
                    }
                }
                if (failed)
                    continue;
                return con.Invoke(argsCopy);
            }

            return Activator.CreateInstance(type, args);
        }

        internal static object CallStaticMethod(
            string name,
            Type type,
            object[] args,
            TypeOptions typeOptions)
        {
            if (!typeOptions.HasFlag(TypeOptions.AllowStaticMethod))
                Exceptions.ThrowCanNotCallStaticMethod(type);
            var list = type.GetMethods(BindingFlags.Public |
                BindingFlags.Static).Where(x => x.Name == name).ToArray();
            var argsLen = args.Length;
            var len = list.Length;
            if (!typeOptions.HasFlag(TypeOptions.AutomaticTypeConversion))
                return Activator.CreateInstance(type, args);
            var convertStringArgumentsToEnum =
                typeOptions.HasFlag(TypeOptions.ConvertStringArgumentsToEnum);
            for (var i = 0; i < len; ++i)
            {
                var con = list[i];
                var argsCopy = args.ToArray();
                var parameters = con.GetParameters();
                var paramLen = parameters.Length;
                if (argsLen != paramLen)
                    continue;
                bool failed = false;
                for (var j = 0; j < paramLen; ++j)
                {
                    var arg = argsCopy[j];
                    var p = parameters[j];
                    if (arg is Undefined)
                    {
                        argsCopy[i] = null;
                        arg = null;
                    }
                    if (arg == null)
                    {
                        if (p.ParameterType.IsClass)
                            continue;
                        failed = true;
                        break;
                    }
                    if (arg.GetType() == p.ParameterType)
                        continue;
                    try
                    {
                        if (convertStringArgumentsToEnum &&
                            p.ParameterType.IsEnum &&
                            arg is string s &&
                            Enum.TryParse(p.ParameterType, s, true, out var enumValue))
                        {
                            argsCopy[j] = enumValue;
                            continue;
                        }
                        argsCopy[j] = Convert.ChangeType(arg, p.ParameterType);
                    }
                    catch (Exception)
                    {
                        failed = true;
                        break;
                    }
                }
                if (failed)
                    continue;
                return con.Invoke(null, argsCopy);
            }
            return null;
        }
        
        internal static bool TryGetStaticMember(string name, Type type, TypeOptions typeOptions, out object result)
        {
            result = null;
            if (!typeOptions.HasFlag(TypeOptions.AllowStaticMethod))
                Exceptions.ThrowCanNotCallStaticMethod(type);
            
            var staticMember = type.GetMember(name, 
                    BindingFlags.Public | BindingFlags.Static);
            if (staticMember.Length == 0)
                return false;
            var sm = staticMember[0];
            if (sm is PropertyInfo p) {
                result = p.GetValue(null);
                return true;
            }
            if (sm is FieldInfo m)
            {
                result = m.GetValue(null);
                return true;
            }                
            return false;
        }
    }
}

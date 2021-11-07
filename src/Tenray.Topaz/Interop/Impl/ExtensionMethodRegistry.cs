using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Tenray.Topaz.Interop
{
    public class ExtensionMethodRegistry
    {
        List<MethodInfo> registeredMethods = new();

        Dictionary<string, MethodAndParameterInfo> methodAndParameterInfoMap;

        HashSet<Guid> registeredGuids = new();

        public IReadOnlyList<MethodInfo> ExtensionMethods => registeredMethods;
        
        public MethodAndParameterInfo GetMethodAndParameterInfo(string name)
        {
            if (methodAndParameterInfoMap == null)
                InitMethodCache();
            if (methodAndParameterInfoMap.TryGetValue(name, out var result))
                return result;
            return MethodAndParameterInfo.Empty;
        }

        private MethodAndParameterInfo GenerateMethodAndParameterInfo(string name)
        {
            var methods = registeredMethods.Where(x => x.Name == name).ToArray();
            if (methods.Length == 0)
                return MethodAndParameterInfo.Empty;
            var result = new MethodAndParameterInfo(
                methods,
                methods.Select(x => x.GetParameters()).ToArray());
            return result;
        }

        public void AddType(Type type)
        {
            var guid = type.GUID;
            if (registeredGuids.Contains(guid))
                return;
            registeredGuids.Add(guid);
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            var len = methods.Length;
            for (var i = 0; i < len; ++i)
            {
                var m = methods[i];
                if (!m.IsDefined(typeof(ExtensionAttribute), true))
                    continue;
                if (m.ContainsGenericParameters)
                {
                    var genericParamCount = m.GetGenericArguments().Length;
                    var genericArgs = Enumerable.Range(0, genericParamCount).Select(x => typeof(object)).ToArray();
                    m = m.MakeGenericMethod(genericArgs);
                }
                registeredMethods.Add(m);
            }
        }

        public void InitMethodCache()
        {
            var allNames = registeredMethods.Select(x => x.Name).Distinct().ToArray();
            methodAndParameterInfoMap = allNames.ToDictionary(x => x, y => GenerateMethodAndParameterInfo(y));
        }
    }
}

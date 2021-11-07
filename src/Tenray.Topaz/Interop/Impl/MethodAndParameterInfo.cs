using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tenray.Topaz.Interop
{
    public class MethodAndParameterInfo
    {
        public readonly IReadOnlyList<MethodInfo> MethodInfos;

        public readonly ParameterInfo[][] ParameterInfos;

        public bool HasAny => MethodInfos.Count > 0;

        public readonly static MethodAndParameterInfo Empty =
            new MethodAndParameterInfo(Array.Empty<MethodInfo>(), Array.Empty<ParameterInfo[]>());

        public MethodAndParameterInfo(
            IReadOnlyList<MethodInfo> methodInfos,
            ParameterInfo[][] parameterInfos)
        {
            MethodInfos = methodInfos;
            ParameterInfos = parameterInfos;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Tenray.Topaz.Interop
{
    public class DefaultMemberAccessPolicy : IMemberAccessPolicy
    {
        TopazEngine TopazEngine;

        static HashSet<string> TypeMemberWhiteList = new ()
        {
            "IsClass",
            "FullName",
            "Namespace",
            "ToString",
            "IsEnum",
            "IsValueType",
            "IsPrimitive",
            "GetTypeCode",
            "GetEnumName",
            "GetEnumNames",
            "GetEnumValues",
            "IsAssignableFrom",
            "IsAssignableTo",
            "IsSubclassOf"
        };

        public DefaultMemberAccessPolicy(TopazEngine topazEngine)
        {
            TopazEngine = topazEngine;
        }

        public bool IsObjectMemberAccessAllowed(object obj, string memberName)
        {
            if (obj == null || memberName == null)
                return true;
            var enableReflection = TopazEngine.Options.SecurityPolicy
                .HasFlag(Options.SecurityPolicy.EnableReflection);
            if (enableReflection)
                return true;
            if (obj is Type)
            {
                return TypeMemberWhiteList.Contains(memberName);
            }
            return true;
        }
    }
}

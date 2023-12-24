using System;
using System.Reflection;

namespace Tenray.Topaz.Interop;

public interface IMemberInfoProvider
{
    MemberInfo[] GetInstanceMembers(object instance, string memberName);

    MemberInfo[] GetStaticMembers(Type type, string memberName);
}
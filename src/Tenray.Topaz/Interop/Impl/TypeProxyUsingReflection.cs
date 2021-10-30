using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public class TypeProxyUsingReflection : ITypeProxy
    {
        public string Name { get; }

        public Type ProxiedType { get; }
        
        public ProxyOptions ProxyOptions { get; }

        readonly ConstructorInfo[] constructors;

        readonly ParameterInfo[][] constructorParameters;
        
        readonly PropertyInfo[] indexedProperties;

        readonly ParameterInfo[][] indexedPropertyParameters;

        public TypeProxyUsingReflection(
            Type proxiedType,
            string name = null,
            ProxyOptions proxyOptions = ProxyOptions.Default)
        {
            Name = name ?? proxiedType.FullName;
            ProxiedType = proxiedType;
            ProxyOptions = proxyOptions;
            if (proxyOptions.HasFlag(ProxyOptions.AllowConstructor) &&
                proxyOptions.HasFlag(ProxyOptions.AutomaticTypeConversion))
            {
                constructors = proxiedType.GetConstructors(
                    BindingFlags.Public | BindingFlags.Instance);
                constructorParameters = constructors
                    .Select(x => x.GetParameters())
                    .ToArray();
            }

            if (proxyOptions.HasFlag(ProxyOptions.AllowProperty))
            {
                {
                    (indexedProperties, indexedPropertyParameters) =
                        IndexedPropertyMetaGetter
                        .GetIndexedPropertiesAndParameters(
                            proxiedType,
                            BindingFlags.Public | BindingFlags.Static);
                }
            }
        }

        public object CallConstructor(IReadOnlyList<object> args)
        {
            var type = ProxiedType;
            var options = ProxyOptions;
            if (!options.HasFlag(ProxyOptions.AllowConstructor))
                Exceptions.ThrowCanNotCallConstructor(type);
            
            if (args.Count == 0)
                return Activator.CreateInstance(type);            
            
            if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
                return Activator.CreateInstance(type, args);

            var convertStringsToEnum =
                options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
            if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
                args,
                constructorParameters,
                convertStringsToEnum,
                out var index,
                out var convertedArgs))
            {
                var bestConstructor = constructors[index];
                return bestConstructor.Invoke(convertedArgs);
            }
            Exceptions
                .ThrowCanNotCallConstructorWithGivenArguments(
                Name, args);
            return null;
        }

        public bool TryGetStaticMember(
            object member,
            out object value,
            bool isIndexedProperty = false)
        {
            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            value = null;
            if (isIndexedProperty)
            {
                if (!allowProperty)
                    return false;
                var convertStringsToEnum =
                    options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    new[] { member },
                    indexedPropertyParameters,
                    convertStringsToEnum,
                    out var index,
                    out var convertedArgs))
                {
                    var indexedProperty = indexedProperties[index];
                    if (indexedProperty.GetMethod == null ||
                        indexedProperty.GetMethod.IsPrivate)
                        return false;
                    value = indexedProperties[index].GetValue(null, convertedArgs);
                    return true;
                }
                return false;
            }

            if (member is not string memberName)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(Name, member);
                return false;
            }

            var members = ProxiedType
                .GetMember(memberName, BindingFlags.Public | BindingFlags.Static);
            if (members.Length == 0)
                return false;
            var firstMember = members[0];
            if (firstMember is PropertyInfo property)
            {
                if (!allowProperty)
                    return false;
                if (property.GetMethod == null ||  
                    property.GetMethod.IsPrivate)
                    return false;
                value = property.GetValue(null);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;
                value = field.GetValue(null);
                return true;
            }
            var allowMethod =
                options.HasFlag(ProxyOptions.AllowMethod);
            if (!allowMethod)
                return false;
            var methods = members
                .Where(x => x is MethodInfo)
                .Cast<MethodInfo>()
                .ToArray();
            if (methods.Length == 0)
                return false;
            value = new InvokerUsingReflection(Name + "." + memberName, methods, null, options);
            return true;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool TrySetStaticMember(
            object member,
            object value,
            bool isIndexedProperty = false)
        {
            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            if (isIndexedProperty)
            {
                if (!allowProperty)
                    return false;
                var convertStringsToEnum =
                    options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    new[] { member },
                    indexedPropertyParameters,
                    convertStringsToEnum,
                    out var index,
                    out var convertedArgs))
                {
                    var indexedProperty = indexedProperties[index];
                    if (indexedProperty.SetMethod == null || 
                        indexedProperty.SetMethod.IsPrivate)
                        return false;
                    indexedProperties[index].SetValue(null, value, convertedArgs);
                    return true;
                }
                return false;
            }

            if (member is not string memberName)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(Name, member);
                return false;
            }

            var members = ProxiedType
                .GetMember(memberName, BindingFlags.Public | BindingFlags.Static);
            if (members.Length == 0)
                return false;
            var firstMember = members[0];
            if (firstMember is PropertyInfo property)
            {
                if (!allowProperty)
                    return false;
                if (property.SetMethod == null || property.SetMethod.IsPrivate)
                    return false;
                property.SetValue(null, value);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;
                field.SetValue(null, value);
                return true;
            }
            return false;
        }
    }
}

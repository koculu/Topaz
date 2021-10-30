using System;
using System.Linq;
using System.Reflection;

namespace Tenray.Topaz.Interop
{
    public static class IndexedPropertyMetaGetter
    {
        public static (PropertyInfo[], ParameterInfo[][])
            GetIndexedPropertiesAndParameters(Type type, BindingFlags bindingFlags)
        {
            // Javascript notation does not let multiple index parameters.
            // Retrieve only single parametered indexes.
            var props =
                    type.GetProperties(bindingFlags)
                    .Where(x => x.GetIndexParameters().Length == 1)
                    .ToArray();
            var parameters = props
                .Select(x => x.GetIndexParameters())
                .ToArray();
            return (props, parameters);
        }
    }
}

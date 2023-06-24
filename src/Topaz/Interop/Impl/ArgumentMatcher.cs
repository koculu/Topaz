using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tenray.Topaz.Interop;

public static class ArgumentMatcher
{
    public static bool TryFindBestMatch(
        IValueConverter valueConverter,
        IReadOnlyList<object> args,
        ParameterInfo[][] allParameters,
        out int index)
    {
        var argsLen = args.Count;
        var allParametersLen = allParameters.Length;
        for (var i = 0; i < allParametersLen; ++i)
        {
            var parameters = allParameters[i];
            var paramLen = parameters.Length;
            if (argsLen != paramLen)
                continue;
            bool failed = false;
            for (var j = 0; j < paramLen; ++j)
            {
                var arg = args[j];
                var p = parameters[j];
                var ptype = p.ParameterType;
                if (valueConverter.IsValueAssignableTo(arg, ptype))
                    continue;
                failed = true;
                break;
            }
            if (failed)
                continue;
            index = i;
            return true;
        }
        index = -1;
        return false;
    }

    public static bool TryFindBestMatchWithTypeConversion(
        IValueConverter valueConverter,
        IReadOnlyList<object> args,
        ParameterInfo[] parameters,
        out object[] convertedArgs)
    {
        var argsLen = args.Count;
        var paramLen = parameters.Length;

        if (paramLen == 0)
        {
            convertedArgs = null;
            return argsLen == 0;
        }

        var hasDefaultValue = false;
        var defaultValueIndex = paramLen;

        for (var j = 0; j < paramLen; ++j)
        {
            var p = parameters[j];
            if (!hasDefaultValue && j > argsLen - 1)
            {
                hasDefaultValue = p.HasDefaultValue;
                if (hasDefaultValue)
                {
                    defaultValueIndex = j;
                }
            }
        }

        var lastParam = parameters[paramLen - 1];
        var hasParamArrayAttribute =
                lastParam.ParameterType.IsArray &&
                lastParam.IsDefined(typeof(ParamArrayAttribute));

        if (!hasParamArrayAttribute && !hasDefaultValue && argsLen != paramLen)
        {
            convertedArgs = null;
            return false;
        }

        var paramArrayAttributeIndex = paramLen;
        if (hasParamArrayAttribute)
        {
            paramArrayAttributeIndex = paramLen - 1;
        }

        var minimumAcceptableArgsLen = Math.Min(paramArrayAttributeIndex, defaultValueIndex);
        if (argsLen < minimumAcceptableArgsLen)
        {
            convertedArgs = null;
            return false;
        }

        IList paramsCollection = null;
        Type paramArrayInnerType = null;
        if (hasParamArrayAttribute)
        {
            paramsCollection = (IList)Activator
                .CreateInstance(lastParam.ParameterType,
                Math.Max(0, argsLen - paramArrayAttributeIndex));
            paramArrayInnerType = lastParam.ParameterType.GetElementType();
        }

        var argsCopy = args.ToArray();
        Type useParamArrayInnerType = null;
        for (var j = 0; j < argsLen; ++j)
        {
            var parametersIndex = j;
            if (j >= paramArrayAttributeIndex)
            {
                parametersIndex = paramArrayAttributeIndex;
                useParamArrayInnerType = paramArrayInnerType;
            }

            var ptype = useParamArrayInnerType ??
                parameters[parametersIndex].ParameterType;
            var arg = argsCopy[j];

            if (valueConverter.TryConvertValue(arg, ptype, out var convertedArg))
            {
                argsCopy[j] = convertedArg;
                continue;
            }
            convertedArgs = null;
            return false;
        }

        if (argsLen < paramLen && hasDefaultValue)
        {
            var filledArgs = new object[paramLen];
            Array.Copy(argsCopy, filledArgs, argsLen);
            for (var k = argsLen; k < paramLen; ++k)
            {
                filledArgs[k] = parameters[k].DefaultValue;
            }
            argsCopy = filledArgs;
        }

        if (hasParamArrayAttribute)
        {
            convertedArgs = new object[paramArrayAttributeIndex + 1];
            Array.Copy(argsCopy, convertedArgs, paramArrayAttributeIndex);
            convertedArgs[paramArrayAttributeIndex] = paramsCollection;
            var z = 0;
            for (var k = paramArrayAttributeIndex; k < argsLen; ++k)
                paramsCollection[z++] = argsCopy[k];
            return true;
        }

        convertedArgs = argsCopy;
        return true;
    }

    public static bool TryFindBestMatchWithTypeConversion(
        IValueConverter valueConverter,
        IReadOnlyList<object> args,
        ParameterInfo[][] allParameters,
        out int index,
        out object[] convertedArgs)
    {
        if (TryFindBestMatch(valueConverter, args, allParameters, out index))
        {
            convertedArgs = args as object[] ?? args.ToArray();
            return true;
        }
        var allParametersLen = allParameters.Length;
        for (var i = 0; i < allParametersLen; ++i)
        {

            var parameters = allParameters[i];
            if (TryFindBestMatchWithTypeConversion(valueConverter, args, parameters, out convertedArgs))
            {
                index = i;
                return true;
            }
        }
        index = -1;
        convertedArgs = null;
        return false;
    }
}

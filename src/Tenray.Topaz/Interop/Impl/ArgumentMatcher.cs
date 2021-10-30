﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tenray.Topaz.Interop
{
    public static class ArgumentMatcher
    {
        public static bool TryFindBestMatch(
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
                    if (arg == null)
                    {
                        if (ptype.IsClass)
                            continue;
                        failed = true;
                        break;
                    }
                    var argType = arg.GetType();
                    if (ptype == typeof(object) ||
                        ptype == argType ||
                        ptype.IsAssignableFrom(argType))
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
            IReadOnlyList<object> args,
            ParameterInfo[][] allParameters,
            bool convertStringsToEnum,
            out int index,
            out object[] convertedArgs)
        {
            if (TryFindBestMatch(args, allParameters, out index))
            {
                convertedArgs = args as object[] ?? args.ToArray();
                return true;
            }
            var argsLen = args.Count;
            var allParametersLen = allParameters.Length;
            for (var i = 0; i < allParametersLen; ++i)
            {
                var parameters = allParameters[i];
                var paramLen = parameters.Length;
                if (argsLen != paramLen)
                    continue;
                var argsCopy = args.ToArray();
                bool failed = false;
                for (var j = 0; j < paramLen; ++j)
                {
                    var arg = argsCopy[j];
                    var p = parameters[j];
                    var ptype = p.ParameterType;
                    if (arg is Undefined)
                    {
                        argsCopy[i] = null;
                        arg = null;
                    }
                    if (arg == null)
                    {
                        if (ptype.IsClass)
                            continue;
                        failed = true;
                        break;
                    }
                    var argType = arg.GetType();
                    if (ptype == typeof(object) ||
                        ptype == argType ||
                        ptype.IsAssignableFrom(argType))
                        continue;
                    try
                    {
                        if (convertStringsToEnum &&
                            ptype.IsEnum &&
                            arg is string s &&
                            Enum.TryParse(ptype, s, true, out var enumValue))
                        {
                            argsCopy[j] = enumValue;
                            continue;
                        }
                        argsCopy[j] = Convert.ChangeType(arg, ptype);
                    }
                    catch (Exception)
                    {
                        failed = true;
                        break;
                    }
                }
                if (failed)
                    continue;
                index = i;
                convertedArgs = argsCopy;
                return true;
            }
            index = -1;
            convertedArgs = null;
            return false;
        }
    }
}
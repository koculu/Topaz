using System;

namespace Tenray.Topaz.Interop
{
    public sealed class DynamicDelagateProxy
    {
        readonly Func<object[], object> ActualFunction;
        
        readonly Type ReturnType;

        public bool HasReturnType { get; }

        readonly IValueConverter ValueConverter;

        public DynamicDelagateProxy(
            Func<object[], object> actualFunction,
            Type returnType,
            IValueConverter valueConverter)
        {
            ActualFunction = actualFunction;
            ReturnType = returnType;
            HasReturnType = returnType != typeof(void);
            ValueConverter = valueConverter;
        }
        
        private object ExecuteByParams(params object[] args)
        {
            var result = ActualFunction(args);
            if (HasReturnType &&
                ValueConverter.TryConvertValue(result, ReturnType, out var convertedValue))
                return convertedValue;
            return result;
        }

        private void CallAction()
        {
            ExecuteByParams();
        }

        private void CallAction(object arg0)
        {
            ExecuteByParams(arg0);
        }

        private void CallAction(object arg0, object arg1)
        {
            ExecuteByParams(arg0, arg1);
        }

        private void CallAction(object arg0, object arg1, object arg2)
        {
            ExecuteByParams(arg0, arg1, arg2);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        private void CallAction(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14)
        {
            ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        private object CallFunc()
        {
            return ExecuteByParams();
        }

        private object CallFunc(object arg0)
        {
            return ExecuteByParams(arg0);
        }

        private object CallFunc(object arg0, object arg1)
        {
            return ExecuteByParams(arg0, arg1);
        }

        private object CallFunc(object arg0, object arg1, object arg2)
        {
            return ExecuteByParams(arg0, arg1, arg2);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        private object CallFunc(object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14)
        {
            return ExecuteByParams(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }
    }
}

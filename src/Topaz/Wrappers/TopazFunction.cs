using Esprima.Ast;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.API;
using Tenray.Topaz.Core;
using Tenray.Topaz.Expressions;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz;

internal sealed class TopazFunction : IConvertible, ITopazFunction
{
    internal ScriptExecutor ScriptExecutor { get; }

    public string Name { get; private set; }

    public int Length => GetArgumentsLength();

    public string this[int index] => GetArgumentName(index);

    FunctionDeclaration expr1;

    FunctionExpression expr2;

    ArrowFunctionExpression expr3;

    internal TopazFunction(ScriptExecutor scriptExecutor, string name, FunctionDeclaration expr)
    {
        ScriptExecutor = scriptExecutor;
        Name = name;
        this.expr1 = expr;
    }

    internal TopazFunction(ScriptExecutor scriptExecutor, string name, FunctionExpression expr)
    {
        ScriptExecutor = scriptExecutor;
        Name = name;
        this.expr2 = expr;
    }

    internal TopazFunction(ScriptExecutor scriptExecutor, string name, ArrowFunctionExpression expr)
    {
        ScriptExecutor = scriptExecutor;
        Name = name;
        this.expr3 = expr;
    }

    public object Execute(IReadOnlyList<object> args, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var scriptExecutor = ScriptExecutor.NewFunctionInnerBlockScope(args);
        var parameters =
            expr1?.Params ??
            expr2?.Params ??
            expr3.Params;

        var len = parameters.Count;
        for (var i = 0; i < len; ++i)
        {
            var p = parameters[i];
            var isRest = false;
            object defaultValue = null;
            // https://exploringjs.com/es6/ch_destructuring.html#sec_destructuring-algorithm                
            if (p is AssignmentPattern assignmentPattern)
            {
                // ex: (x = 1) => x * x
                p = assignmentPattern.Left;
                defaultValue = scriptExecutor
                    .ExecuteExpressionAndGetValue(assignmentPattern.Right, token);
            }
            else if (p is RestElement r)
            {
                p = r.Argument;
                isRest = true;
            }

            if (p is ObjectPattern objectPattern)
            {
                // dont process Object Pattern for missing arguments!
                if (defaultValue == null && i >= args.Count)
                    continue;
                ObjectPatternHandler.ProcessObjectPattern(
                    scriptExecutor,
                    objectPattern,
                    i < args.Count ? args[i] : defaultValue,
                    (x, y, token) =>
                    {
                        scriptExecutor.DefineVariable(x, y, VariableKind.Var);
                    },
                    token);
                continue;
            }
            var id = (Identifier)p;
            if (isRest)
            {
                var restLen = args.Count - i;

                var rested = restLen < 0 ?
                    new JsArray(0) :
                    new JsArray(restLen);
                for (var j = 0; j < restLen; ++j)
                {
                    rested.AddArrayValue(args[i + j]);
                }
                scriptExecutor.DefineVariable(id,
                    rested,
                    VariableKind.Var);
            }
            else
            {
                scriptExecutor.DefineVariable(id,
                    i < args.Count ? args[i] : defaultValue,
                    VariableKind.Var);
            }
        }

        var result = scriptExecutor.ExecuteStatement(
            expr1?.Body ??
            expr2?.Body ??
            expr3.Body, token);

        if (result is ReturnWrapper ret)
        {
            scriptExecutor.ReturnToPool();
            return ret.Result;
        }
        if (result is Expression expr)
        {
            var returnValue = scriptExecutor.ExecuteExpressionAndGetValue(expr, token);
            scriptExecutor.ReturnToPool();
            return returnValue;
        }
        else
        {
            var returnValue = scriptExecutor.GetValue(result);
            scriptExecutor.ReturnToPool();
            return returnValue;
        }
    }

    public async ValueTask<object> ExecuteAsync(IReadOnlyList<object> args, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var scriptExecutor = ScriptExecutor.NewFunctionInnerBlockScope(args);
        var parameters =
            expr1?.Params ??
            expr2?.Params ??
            expr3.Params;

        var len = parameters.Count;
        for (var i = 0; i < len; ++i)
        {
            var p = parameters[i];
            var isRest = false;
            object defaultValue = null;
            // https://exploringjs.com/es6/ch_destructuring.html#sec_destructuring-algorithm                
            if (p is AssignmentPattern assignmentPattern)
            {
                // ex: (x = 1) => x * x
                p = assignmentPattern.Left;
                defaultValue = await scriptExecutor
                    .ExecuteExpressionAndGetValueAsync(assignmentPattern.Right, token);
            }
            else if (p is RestElement r)
            {
                p = r.Argument;
                isRest = true;
            }

            if (p is ObjectPattern objectPattern)
            {
                // dont process Object Pattern for missing arguments!
                if (defaultValue == null && i >= args.Count)
                    continue;
                await ObjectPatternHandler.ProcessObjectPatternAsync(
                    scriptExecutor,
                    objectPattern,
                    i < args.Count ? args[i] : defaultValue,
                    (x, y, token) =>
                    {
                        scriptExecutor.DefineVariable(x, y, VariableKind.Var);
                        return default;
                    },
                    token);
                continue;
            }
            var id = (Identifier)p;
            if (isRest)
            {
                var restLen = args.Count - i;

                var rested = restLen < 0 ?
                    new JsArray(0) :
                    new JsArray(restLen);
                for (var j = 0; j < restLen; ++j)
                {
                    rested.AddArrayValue(args[i + j]);
                }
                scriptExecutor.DefineVariable(id,
                    rested,
                    VariableKind.Var);
            }
            else
            {
                scriptExecutor.DefineVariable(id,
                    i < args.Count ? args[i] : defaultValue,
                    VariableKind.Var);
            }
        }

        var result = await scriptExecutor.ExecuteStatementAsync(
            expr1?.Body ??
            expr2?.Body ??
            expr3.Body, token);

        if (result is ReturnWrapper ret)
            return ret.Result;

        if (result is Expression expr)
            return await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr, token);
        return scriptExecutor.GetValue(result);
    }

    public TypeCode GetTypeCode()
    {
        throw new NotSupportedException();
    }

    public bool ToBoolean(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public byte ToByte(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public char ToChar(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public DateTime ToDateTime(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public decimal ToDecimal(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public double ToDouble(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public short ToInt16(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public int ToInt32(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public long ToInt64(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public float ToSingle(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public string ToString(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public uint ToUInt32(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
        throw new NotSupportedException();
    }

    public object ToType(Type conversionType, IFormatProvider provider)
    {
        if (!conversionType.IsSubclassOf(typeof(Delegate)))
            throw new NotSupportedException();

        var topazParameters =
           expr1?.Params ??
           expr2?.Params ??
           expr3.Params;

        var argTypes = conversionType.GetTypeInfo().GenericTypeArguments;
        var returnType = conversionType.GetMethod("Invoke").ReturnType;
        var isVoid = returnType == typeof(void);
        if (isVoid && argTypes.Length != topazParameters.Count ||
            !isVoid && argTypes.Length - 1 != topazParameters.Count
            )
            throw new NotSupportedException();
        return DynamicDelagateFactory.CreateDynamicDelegate(argTypes, returnType,
            (args) => Execute(args, default), ScriptExecutor.TopazEngine.ValueConverter);
    }

    int GetArgumentsLength()
    {
        var parameters =
            expr1?.Params ??
            expr2?.Params ??
            expr3.Params;
        return parameters.Count;
    }

    string GetArgumentName(int index)
    {
        var parameters =
            expr1?.Params ??
            expr2?.Params ??
            expr3.Params;
        if (index < 0 || index >= parameters.Count)
            return null;

        var p = parameters[index];
        if (p is AssignmentPattern assignmentPattern)
        {
            // ex: (x = 1) => x * x
            p = assignmentPattern.Left;
        }
        else if (p is RestElement r)
        {
            p = r.Argument;
        }

        if (p is ObjectPattern)
        {
            // Object pattern argument string extraction is not supported!
            return null;
        }
        var id = p as Identifier;
        if (id == null)
            return null;
        return id.Name;
    }

    public object Invoke(CancellationToken token, params object[] args) => Execute(args, token);

    public ValueTask<object> InvokeAsync(CancellationToken token, params object[] args)
        => ExecuteAsync(args, token);
}

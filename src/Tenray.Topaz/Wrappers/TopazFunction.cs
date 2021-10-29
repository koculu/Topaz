using Esprima.Ast;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.Expressions;

namespace Tenray.Topaz
{
    internal class TopazFunction
    {
        internal ScriptExecutor ScriptExecutor { get; }

        internal string Name { get; private set; }

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

        internal object Execute(IReadOnlyList<object> args)
        {
            var scriptExecutor = ScriptExecutor.NewFunctionInnerBlockScope();
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
                        .ExecuteExpressionAndGetValue(assignmentPattern.Right);
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
                        (x, y) =>
                        {
                            scriptExecutor.DefineVariable(x, y, VariableKind.Var);
                        });
                    continue;
                }
                var id = (Identifier)p;
                if (isRest)
                {
                    var restLen = args.Count - i;
                    
                    var rested = restLen < 0 ? 
                        Array.Empty<object>() :
                        new object[restLen];
                    for (var j = 0; j < restLen; ++j)
                    {
                        rested[j] = args[i + j];
                    }
                    scriptExecutor.DefineVariable(id.Name,
                        rested,
                        VariableKind.Var);
                }
                else
                {
                    scriptExecutor.DefineVariable(id.Name,
                        i < args.Count ? args[i] : defaultValue,
                        VariableKind.Var);
                }
            }
            
            var result = scriptExecutor.ExecuteStatement(
                expr1?.Body ??
                expr2?.Body ??
                expr3.Body);

            if (result is ReturnWrapper ret)
                return ret.Result;

            if (result is Expression expr)
                return scriptExecutor.ExecuteExpressionAndGetValue(expr);
            return result;
        }

        internal async ValueTask<object> ExecuteAsync(IReadOnlyList<object> args)
        {
            var scriptExecutor = ScriptExecutor.NewFunctionInnerBlockScope();
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
                        .ExecuteExpressionAndGetValueAsync(assignmentPattern.Right);
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
                        (x, y) =>
                        {
                            scriptExecutor.DefineVariable(x, y, VariableKind.Var);
                            return ValueTask.CompletedTask;
                        });
                    continue;
                }
                var id = (Identifier)p;
                if (isRest)
                {
                    var restLen = args.Count - i;

                    var rested = restLen < 0 ?
                        Array.Empty<object>() :
                        new object[restLen];
                    for (var j = 0; j < restLen; ++j)
                    {
                        rested[j] = args[i + j];
                    }
                    scriptExecutor.DefineVariable(id.Name,
                        rested,
                        VariableKind.Var);
                }
                else
                {
                    scriptExecutor.DefineVariable(id.Name,
                        i < args.Count ? args[i] : defaultValue,
                        VariableKind.Var);
                }
            }

            var result = await scriptExecutor.ExecuteStatementAsync(
                expr1?.Body ??
                expr2?.Body ??
                expr3.Body);

            if (result is ReturnWrapper ret)
                return ret.Result;

            if (result is Expression expr)
                return await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr);
            return result;
        }
    }
}

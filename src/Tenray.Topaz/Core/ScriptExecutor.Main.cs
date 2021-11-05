using Esprima.Ast;
using Microsoft.Collections.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Expressions;
using Tenray.Topaz.Options;
using Tenray.Topaz.Statements;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor
    {
        private static int lastScriptExecutorId = 0;

        public int Id { get; }

        private bool isEmptyScope = true;

        /// <summary>
        /// A readonly scope prevents:
        /// - defining new variables
        /// - deleting variables
        /// - updating variable values
        /// </summary>
        internal bool IsReadOnly { get; set; }

        /// <summary>
        /// A frozen scope can not accept new variable definitions,
        /// however updating values of existing variables
        /// is possible.
        /// Function Scope (Closure) is a frozen scope.
        /// </summary>
        internal bool IsFrozen { get; set; }

        internal bool IsThreadSafeScope { get; }

        internal TopazEngine TopazEngine { get; }

        internal ScopeType ScopeType { get; }

        internal ScriptExecutor GlobalScope { get; }

        internal ScriptExecutor ParentScope { get; }

        /// <summary>
        /// Variables dictionary that support thread safe access.
        /// </summary>
        internal ConcurrentDictionary<string, Variable> SafeVariables;
        
        /// <summary>
        /// Variables dictionary for scopes that does not require thread safety.
        /// Provides faster variable access.
        /// </summary>
        internal DictionarySlim<string, Variable> UnsafeVariables;

        internal TopazEngineOptions Options => TopazEngine.Options;

        internal ScriptExecutor(TopazEngine topazEngine, bool isThreadSafe = true)
        {
            Id = Interlocked.Increment(ref lastScriptExecutorId);
            TopazEngine = topazEngine;
            ScopeType = ScopeType.Global;
            GlobalScope = this;
            IsThreadSafeScope = isThreadSafe;
            if (IsThreadSafeScope)
                SafeVariables = new();
            else
                UnsafeVariables = new();
        }

        internal ScriptExecutor(TopazEngine topazEngine, ScriptExecutor parentScope, bool isThreadSafe)
        {
            Id = Interlocked.Increment(ref lastScriptExecutorId);
            TopazEngine = topazEngine;
            ScopeType = ScopeType.Custom;
            GlobalScope = this;
            ParentScope = parentScope;
            IsThreadSafeScope = isThreadSafe;
            if (IsThreadSafeScope)
                SafeVariables = new();
            else
                UnsafeVariables = new();
        }

        internal ScriptExecutor(TopazEngine topazEngine, ScriptExecutor parentScope, ScopeType scope)
        {
            Id = Interlocked.Increment(ref lastScriptExecutorId);
            TopazEngine = topazEngine;
            var globalScope = parentScope.GlobalScope;
            GlobalScope = globalScope;
            ScopeType = scope;
            ParentScope = parentScope;
            /*
             * If thread safe option has given,
             * Global Scope
             * should be thread safe because mutliple threads
             * can access their content.
             * There is absolutely no way,
             * for Block, FunctionInnerBlock scopes are
             * accessed by multiple threads.
             * The Function scope (Closure) can be accessed by multiple threads.
             * For this reason, it is marked as a frozen scope.
             * Frozen scope's variable state is frozen.
             * Hence, Block, FunctionInnerBlock and Function scopes
             * can be unsafe.
             * Following line disables thread safety
             * for these type of scopes.
             * Note that scope without thread safety
             * uses optimized Dictionary for variable access.
             */
            IsThreadSafeScope = 
                globalScope.IsThreadSafeScope &&
                scope != ScopeType.Block &&
                scope != ScopeType.Function &&
                scope != ScopeType.FunctionInnerBlock;

            if (IsThreadSafeScope)
                SafeVariables = new();
            else
                UnsafeVariables = new();

            if (scope == ScopeType.Function)
            {
                CaptureVariables();
            }
        }

        internal void ExecuteScript(Script script)
        {
            var nodes = script.Body;
            var len = nodes.Count;
            for (var i = 0; i < len; ++i)
            {
                var el = nodes[i];
                ExecuteStatement(el);
            }
            return;
        }

        internal async ValueTask ExecuteScriptAsync(Script script)
        {
            var nodes = script.Body;
            var len = nodes.Count;
            for (var i = 0; i < len; ++i)
            {
                var el = nodes[i];
                await ExecuteStatementAsync(el);
            }
        }

        internal ScriptExecutor NewBlockScope()
        {
            return new ScriptExecutor(TopazEngine, this, ScopeType.Block);
        }

        internal ScriptExecutor NewFunctionScope()
        {
            return new ScriptExecutor(TopazEngine, this, ScopeType.Function);
        }

        internal ScriptExecutor NewCustomScope(bool? isThreadSafe)
        {
            if (!isThreadSafe.HasValue)
                isThreadSafe = IsThreadSafeScope;
            return new ScriptExecutor(TopazEngine, this, isThreadSafe.Value);
        }

        internal ScriptExecutor NewFunctionInnerBlockScope()
        {
            return new ScriptExecutor(TopazEngine, this, ScopeType.FunctionInnerBlock);
        }

        internal object ExecuteExpressionAndGetValue(Expression expression)
        {
            return GetValue(ExecuteStatement(expression));
        }

        internal async ValueTask<object> ExecuteExpressionAndGetValueAsync(Expression expression)
        {
            var value = await ExecuteStatementAsync(expression);
            return GetValue(value);
        }

        internal object ExecuteStatement(Node statement)
        {
            if (statement == null)
                return GetNullOrUndefined();
            return statement.Type switch
            {
                Nodes.Literal => LiteralHandler.Execute(this, statement),
                Nodes.Identifier => ((Identifier)statement).TopazIdentifier,
                Nodes.AssignmentExpression => AssignmentExpressionHandler.Execute(this, statement),
                Nodes.ArrayExpression => ArrayExpressionHandler.Execute(this, statement),
                Nodes.BinaryExpression => BinaryExpressionHandler.Execute(this, statement),
                Nodes.CallExpression => CallExpressionHandler.Execute(this, statement),
                Nodes.ChainExpression => ChainExpressionHandler.Execute(this, statement),
                Nodes.ConditionalExpression => ConditionalExpressionHandler.Execute(this, statement),
                Nodes.FunctionExpression => FunctionExpressionHandler.Execute(this, statement),
                Nodes.LogicalExpression => BinaryExpressionHandler.Execute(this, statement),
                Nodes.MemberExpression => MemberExpressionHandler.Execute(this, statement),
                Nodes.ObjectExpression => ObjectExpressionHandler.Execute(this, statement),
                Nodes.SequenceExpression => SequenceExpressionHandler.Execute(this, statement),
                Nodes.UnaryExpression => UnaryExpressionHandler.Execute(this, statement),
                Nodes.UpdateExpression => UpdateExpressionHandler.Execute(this, statement),
                Nodes.TemplateLiteral => TemplateLiteralHandler.Execute(this, statement),
                Nodes.TaggedTemplateExpression => TaggedTemplateExpressionHandler.Execute(this, statement),
                Nodes.ArrowFunctionExpression => ArrowFunctionExpressionHandler.Execute(this, statement),
                Nodes.AwaitExpression => AwaitExpressionHandler.Execute(this, statement),
                Nodes.AssignmentPattern => AssignmentPatternHandler.Execute(this, statement),
                Nodes.NewExpression => NewExpressionHandler.Execute(this, statement),
                
                Nodes.ReturnStatement => new ReturnWrapper(
                    ExecuteExpressionAndGetValue(((ReturnStatement)statement).Argument)),
                Nodes.BreakStatement => BreakWrapper.Instance,
                Nodes.ContinueStatement => ContinueWrapper.Instance,

                Nodes.BlockStatement => BlockStatementHandler.Execute(this, statement),
                Nodes.DoWhileStatement => DoWhileStatementHandler.Execute(this, statement),
                Nodes.ExpressionStatement => ExecuteStatement(((ExpressionStatement)statement).Expression),
                Nodes.ForStatement => ForStatementHandler.Execute(this, statement),
                Nodes.ForInStatement => ForInStatementHandler.Execute(this, statement),
                Nodes.FunctionDeclaration => FunctionDeclarationHandler.Execute(this, statement),
                Nodes.IfStatement => IfStatementHandler.Execute(this, statement),
                Nodes.SwitchStatement => SwitchStatementHandler.Execute(this, statement),
                Nodes.ThrowStatement => ThrowStatementHandler.Execute(this, statement),
                Nodes.TryStatement => TryStatementHandler.Execute(this, statement),
                Nodes.VariableDeclaration => VariableDeclarationHandler.Execute(this, statement),
                Nodes.WhileStatement => WhileStatementHandler.Execute(this, statement),
                Nodes.ForOfStatement => ForOfStatementHandler.Execute(this, statement),
                Nodes.ValueWrapper => ((ValueWrapper)statement).Value,
                // Nodes.CatchClause => DONE,
                // Nodes.Program => DONE,
                // Nodes.Property => DONE,
                // Nodes.RestElement => DONE,
                // Nodes.SwitchCase => DONE,
                // Nodes.VariableDeclarator => DONE,
                // Nodes.ArrayPattern => DONE,
                // Nodes.SpreadElement => DONE,
                // Nodes.TemplateElement => DONE,
                // Nodes.ObjectPattern => DONE,
                // Nodes.ArrowParameterPlaceHolder => never appear in the final tree and only used during the construction of a tree,
                // Nodes.EmptyStatement => nop,

                // Nodes.DebuggerStatement => ThrowNotImplemented(this, statement),
                // Nodes.Import => ThrowNotImplemented(this, statement),
                // Nodes.LabeledStatement => ThrowNotImplemented(this, statement),
                // Nodes.WithStatement => ThrowNotImplemented(this, statement),

                // Nodes.MetaProperty => ThrowNotImplemented(this, statement),
                // Nodes.MethodDefinition => ThrowNotImplemented(this, statement),
                // Nodes.Super => ThrowNotImplemented(this, statement),
                // Nodes.ClassBody => ThrowNotImplemented(this, statement),
                // Nodes.ClassDeclaration => ThrowNotImplemented(this, statement),
                // Nodes.ImportSpecifier => ThrowNotImplemented(this, statement),
                // Nodes.ImportDefaultSpecifier => ThrowNotImplemented(this, statement),
                // Nodes.ImportNamespaceSpecifier => ThrowNotImplemented(this, statement),
                // Nodes.ImportDeclaration => ThrowNotImplemented(this, statement),
                // Nodes.ExportSpecifier => ThrowNotImplemented(this, statement),
                // Nodes.ExportNamedDeclaration => ThrowNotImplemented(this, statement),
                // Nodes.ExportAllDeclaration => ThrowNotImplemented(this, statement),
                // Nodes.ExportDefaultDeclaration => ThrowNotImplemented(this, statement),
                // Nodes.ThisExpression => ThrowNotImplemented(this, statement),
                // Nodes.ClassExpression => ThrowNotImplemented(this, statement),
                // Nodes.YieldExpression => ThrowNotImplemented(this, statement),
                _ => ThrowNotImplemented(this, statement)
            };
        }
        internal async ValueTask<object> ExecuteStatementAsync(Node statement)
        {
            if (statement == null)
                return GetNullOrUndefined();
            return statement.Type switch
            {
                Nodes.Literal => LiteralHandler.Execute(this, statement),
                Nodes.Identifier => ((Identifier)statement).TopazIdentifier,
                Nodes.AssignmentExpression => await AssignmentExpressionHandler.ExecuteAsync(this, statement),
                Nodes.ArrayExpression => await ArrayExpressionHandler.ExecuteAsync(this, statement),
                Nodes.BinaryExpression => await BinaryExpressionHandler.ExecuteAsync(this, statement),
                Nodes.CallExpression => await CallExpressionHandler.ExecuteAsync(this, statement),
                Nodes.ChainExpression => await ChainExpressionHandler.ExecuteAsync(this, statement),
                Nodes.ConditionalExpression => await ConditionalExpressionHandler.ExecuteAsync(this, statement),
                Nodes.FunctionExpression => FunctionExpressionHandler.Execute(this, statement),
                Nodes.LogicalExpression => await BinaryExpressionHandler.ExecuteAsync(this, statement),
                Nodes.MemberExpression => await MemberExpressionHandler.ExecuteAsync(this, statement),
                Nodes.ObjectExpression => await ObjectExpressionHandler.ExecuteAsync(this, statement),
                Nodes.SequenceExpression => await SequenceExpressionHandler.ExecuteAsync(this, statement),
                Nodes.UnaryExpression => await UnaryExpressionHandler.ExecuteAsync(this, statement),
                Nodes.UpdateExpression => await UpdateExpressionHandler.ExecuteAsync(this, statement),
                Nodes.TemplateLiteral => await TemplateLiteralHandler.ExecuteAsync(this, statement),
                Nodes.TaggedTemplateExpression => await TaggedTemplateExpressionHandler.ExecuteAsync(this, statement),
                Nodes.ArrowFunctionExpression => ArrowFunctionExpressionHandler.Execute(this, statement),
                Nodes.AwaitExpression => await AwaitExpressionHandler.ExecuteAsync(this, statement),
                Nodes.AssignmentPattern => await AssignmentPatternHandler.ExecuteAsync(this, statement),
                Nodes.NewExpression => await NewExpressionHandler.ExecuteAsync(this, statement),

                Nodes.ReturnStatement => new ReturnWrapper(
                    await ExecuteExpressionAndGetValueAsync(((ReturnStatement)statement).Argument)),
                Nodes.BreakStatement => BreakWrapper.Instance,
                Nodes.ContinueStatement => ContinueWrapper.Instance,

                Nodes.BlockStatement => await BlockStatementHandler.ExecuteAsync(this, statement),
                Nodes.DoWhileStatement => await DoWhileStatementHandler.ExecuteAsync(this, statement),
                Nodes.ExpressionStatement => await ExecuteStatementAsync(((ExpressionStatement)statement).Expression),
                Nodes.ForStatement => await ForStatementHandler.ExecuteAsync(this, statement),
                Nodes.ForInStatement => await ForInStatementHandler.ExecuteAsync(this, statement),
                Nodes.FunctionDeclaration => FunctionDeclarationHandler.Execute(this, statement),
                Nodes.IfStatement => await IfStatementHandler.ExecuteAsync(this, statement),
                
                Nodes.SwitchStatement => await SwitchStatementHandler.ExecuteAsync(this, statement),
                Nodes.ThrowStatement => await ThrowStatementHandler.ExecuteAsync(this, statement),
                Nodes.TryStatement => await TryStatementHandler.ExecuteAsync(this, statement),
                Nodes.VariableDeclaration => await VariableDeclarationHandler.ExecuteAsync(this, statement),
                Nodes.WhileStatement => await WhileStatementHandler.ExecuteAsync(this, statement),
                Nodes.ForOfStatement => await ForOfStatementHandler.ExecuteAsync(this, statement),
                Nodes.ValueWrapper => ((ValueWrapper)statement).Value,
                _ => ThrowNotImplemented(this, statement)
            };
        }

        private object ThrowNotImplemented(ScriptExecutor scriptExecutor, Node statement)
        {
            return Exceptions.ThrowNotImplemented(statement, this);
        }
    }
}

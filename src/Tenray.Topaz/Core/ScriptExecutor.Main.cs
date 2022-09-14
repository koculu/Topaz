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
    internal sealed partial class ScriptExecutor
    {
        private static int lastScriptExecutorId = 0;

        public int Id { get; private set; }

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

        internal bool IsThreadSafeScope { get; private set; }

        internal TopazEngine TopazEngine { get; private set; }

        internal ScopeType ScopeType { get; private set; }

        internal ScriptExecutor GlobalScope { get; private set; }

        internal ScriptExecutor ParentScope { get; private set; }

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
            Reconstruct(topazEngine, parentScope, scope);
        }

        internal void Reconstruct(TopazEngine topazEngine, ScriptExecutor parentScope, ScopeType scope)
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
            {
                UnsafeVariables = null;
                if (SafeVariables == null)
                    SafeVariables = new();
                else
                    SafeVariables.Clear();

            }
            else
            {
                if (UnsafeVariables == null)
                    UnsafeVariables = new();
                else
                    UnsafeVariables.Clear();
                SafeVariables = null;
            }

            if (scope == ScopeType.Function)
            {
                CaptureVariables();
            }
        }

        internal void ExecuteScript(Script script, CancellationToken token)
        {
            var nodes = script.Body;
            var len = nodes.Count;
            for (var i = 0; i < len; ++i)
            {
                var el = nodes[i];
                ExecuteStatement(el, token);
            }
            return;
        }

        internal async ValueTask ExecuteScriptAsync(Script script, CancellationToken token)
        {
            var nodes = script.Body;
            var len = nodes.Count;
            for (var i = 0; i < len; ++i)
            {
                var el = nodes[i];
                await ExecuteStatementAsync(el, token);
            }
        }

        private bool CanReturnToPool = false;

        private void MarkCanNotReturnToPool()
        {
            var scope = this;
            while (scope != null) {
                scope.CanReturnToPool = false;
                scope = scope.ParentScope;
            }
        }

        internal void ReturnToPool()
        {
            if (CanReturnToPool)
                TopazEngine.ScriptExecutorPool.Return(this);
        }

        internal ScriptExecutor NewBlockScope()
        {
            var result = TopazEngine.ScriptExecutorPool.Get(TopazEngine, this, ScopeType.Block);
            result.CanReturnToPool = true;
            return result;
        }

        internal ScriptExecutor NewFunctionScope()
        {
            // Closures cannot return to pool. Handled in CaptureVariables function.
            // Any other scope can return to pool safely.
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
            var result = TopazEngine.ScriptExecutorPool.Get(TopazEngine, this, ScopeType.FunctionInnerBlock);
            result.CanReturnToPool = true;
            return result;
        }

        internal object ExecuteExpressionAndGetValue(Expression expression, CancellationToken token)
        {
            return GetValue(ExecuteStatement(expression, token));
        }

        internal async ValueTask<object> ExecuteExpressionAndGetValueAsync(Expression expression, CancellationToken token)
        {
            var value = await ExecuteStatementAsync(expression, token);
            return GetValue(value);
        }

        internal object ExecuteStatement(Node statement, CancellationToken token)
        {
            if (statement == null)
                return GetNullOrUndefined();
            return statement.Type switch
            {
                Nodes.Literal => LiteralHandler.Execute(this, statement),
                Nodes.Identifier => ((Identifier)statement).TopazIdentifier,
                Nodes.AssignmentExpression => AssignmentExpressionHandler.Execute(this, statement, token),
                Nodes.ArrayExpression => ArrayExpressionHandler.Execute(this, statement, token),
                Nodes.BinaryExpression => BinaryExpressionHandler.Execute(this, statement, token),
                Nodes.CallExpression => CallExpressionHandler.Execute(this, statement, token),
                Nodes.ChainExpression => ChainExpressionHandler.Execute(this, statement, token),
                Nodes.ConditionalExpression => ConditionalExpressionHandler.Execute(this, statement, token),
                Nodes.FunctionExpression => FunctionExpressionHandler.Execute(this, statement),
                Nodes.LogicalExpression => BinaryExpressionHandler.Execute(this, statement, token),
                Nodes.MemberExpression => MemberExpressionHandler.Execute(this, statement, token),
                Nodes.ObjectExpression => ObjectExpressionHandler.Execute(this, statement, token),
                Nodes.SequenceExpression => SequenceExpressionHandler.Execute(this, statement, token),
                Nodes.UnaryExpression => UnaryExpressionHandler.Execute(this, statement, token),
                Nodes.UpdateExpression => UpdateExpressionHandler.Execute(this, statement, token),
                Nodes.TemplateLiteral => TemplateLiteralHandler.Execute(this, statement, token),
                Nodes.TaggedTemplateExpression => TaggedTemplateExpressionHandler.Execute(this, statement, token),
                Nodes.ArrowFunctionExpression => ArrowFunctionExpressionHandler.Execute(this, statement),
                Nodes.AwaitExpression => AwaitExpressionHandler.Execute(this, statement, token),
                Nodes.AssignmentPattern => AssignmentPatternHandler.Execute(this, statement, token),
                Nodes.NewExpression => NewExpressionHandler.Execute(this, statement, token),
                
                Nodes.ReturnStatement => new ReturnWrapper(
                    ExecuteExpressionAndGetValue(((ReturnStatement)statement).Argument, token)),
                Nodes.BreakStatement => BreakWrapper.Instance,
                Nodes.ContinueStatement => ContinueWrapper.Instance,

                Nodes.BlockStatement => BlockStatementHandler.Execute(this, statement, token),
                Nodes.DoWhileStatement => DoWhileStatementHandler.Execute(this, statement, token),
                Nodes.ExpressionStatement => ExecuteStatement(((ExpressionStatement)statement).Expression, token),
                Nodes.ForStatement => ForStatementHandler.Execute(this, statement, token),
                Nodes.ForInStatement => ForInStatementHandler.Execute(this, statement, token),
                Nodes.FunctionDeclaration => FunctionDeclarationHandler.Execute(this, statement),
                Nodes.IfStatement => IfStatementHandler.Execute(this, statement, token),
                Nodes.SwitchStatement => SwitchStatementHandler.Execute(this, statement, token),
                Nodes.ThrowStatement => ThrowStatementHandler.Execute(this, statement, token),
                Nodes.TryStatement => TryStatementHandler.Execute(this, statement, token),
                Nodes.VariableDeclaration => VariableDeclarationHandler.Execute(this, statement, token),
                Nodes.WhileStatement => WhileStatementHandler.Execute(this, statement, token),
                Nodes.ForOfStatement => ForOfStatementHandler.Execute(this, statement, token),
                Nodes.ValueWrapper => ((ValueWrapper)statement).Value,
                Nodes.EmptyStatement => GetNullOrUndefined(),
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
        internal async ValueTask<object> ExecuteStatementAsync(Node statement, CancellationToken token)
        {
            if (statement == null)
                return GetNullOrUndefined();
            return statement.Type switch
            {
                Nodes.Literal => LiteralHandler.Execute(this, statement),
                Nodes.Identifier => ((Identifier)statement).TopazIdentifier,
                Nodes.AssignmentExpression => await AssignmentExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.ArrayExpression => await ArrayExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.BinaryExpression => await BinaryExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.CallExpression => await CallExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.ChainExpression => await ChainExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.ConditionalExpression => await ConditionalExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.FunctionExpression => FunctionExpressionHandler.Execute(this, statement),
                Nodes.LogicalExpression => await BinaryExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.MemberExpression => await MemberExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.ObjectExpression => await ObjectExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.SequenceExpression => await SequenceExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.UnaryExpression => await UnaryExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.UpdateExpression => await UpdateExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.TemplateLiteral => await TemplateLiteralHandler.ExecuteAsync(this, statement, token),
                Nodes.TaggedTemplateExpression => await TaggedTemplateExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.ArrowFunctionExpression => ArrowFunctionExpressionHandler.Execute(this, statement),
                Nodes.AwaitExpression => await AwaitExpressionHandler.ExecuteAsync(this, statement, token),
                Nodes.AssignmentPattern => await AssignmentPatternHandler.ExecuteAsync(this, statement, token),
                Nodes.NewExpression => await NewExpressionHandler.ExecuteAsync(this, statement, token),

                Nodes.ReturnStatement => new ReturnWrapper(
                    await ExecuteExpressionAndGetValueAsync(((ReturnStatement)statement).Argument, token)),
                Nodes.BreakStatement => BreakWrapper.Instance,
                Nodes.ContinueStatement => ContinueWrapper.Instance,

                Nodes.BlockStatement => await BlockStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.DoWhileStatement => await DoWhileStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.ExpressionStatement => await ExecuteStatementAsync(((ExpressionStatement)statement).Expression, token),
                Nodes.ForStatement => await ForStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.ForInStatement => await ForInStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.FunctionDeclaration => FunctionDeclarationHandler.Execute(this, statement),
                Nodes.IfStatement => await IfStatementHandler.ExecuteAsync(this, statement, token),
                
                Nodes.SwitchStatement => await SwitchStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.ThrowStatement => await ThrowStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.TryStatement => await TryStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.VariableDeclaration => await VariableDeclarationHandler.ExecuteAsync(this, statement, token),
                Nodes.WhileStatement => await WhileStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.ForOfStatement => await ForOfStatementHandler.ExecuteAsync(this, statement, token),
                Nodes.ValueWrapper => ((ValueWrapper)statement).Value,
                Nodes.EmptyStatement => GetNullOrUndefined(),
                _ => ThrowNotImplemented(this, statement)
            };
        }

        private object ThrowNotImplemented(ScriptExecutor scriptExecutor, Node statement)
        {
            return Exceptions.ThrowNotImplemented(statement, this);
        }
    }
}

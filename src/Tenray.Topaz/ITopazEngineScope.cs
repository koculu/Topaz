namespace Tenray.Topaz
{
    public interface ITopazEngineScope
    {
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
        /// eg: Function Scope (Closure) is a frozen scope.
        /// </summary>
        internal bool IsFrozen { get; set; }

        /// <summary>
        /// Returns true if this is the global scope.
        /// </summary>
        internal bool IsGlobalScope { get; }

        /// <summary>
        /// Creates a new child scope.
        /// It is useful to isolate your script execution from global scope.
        /// </summary>
        /// <param name="isThreadSafe">
        /// if true the thread safe child scope is returned,
        /// if false non-thread safe child scope is returned,
        /// if ommitted, parent scope thread safety option is used.</param>
        /// <returns>The new child scope of this scope.</returns>
        ITopazEngineScope NewChildScope(bool? isThreadSafe = null);

        /// <summary>
        /// Executes the script in the scope.
        /// </summary>
        /// <param name="code"></param>
        void ExecuteScript(string code);

        /// <summary>
        /// Executes the single line expression in the scope.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        object ExecuteExpression(string code);

        /// <summary>
        /// Executes the function that is declared in the scope with given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object InvokeFunction(string name, params object[] args);

        /// <summary>
        /// Gets the value of the variable that is defined in the scope
        /// or its parent scopes.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns></returns>
        object GetValue(string name);

        /// <summary>
        /// Defines a new variable with given name and value
        /// or updates existing variable with given name and value
        /// in the scope.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value</param>
        void SetValue(string name, object value);

        /// <summary>
        /// Defines a new variable with given name, value and kind
        /// or updates existing variable with given name, value and kind
        /// in the global scope.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value</param>
        /// <param name="variableKind">Variable kind, var, let or const.</param>
        void SetValueAndKind(string name, object value, VariableKind variableKind);
    }
}

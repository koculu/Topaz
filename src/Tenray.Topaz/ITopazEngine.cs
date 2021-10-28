using Esprima;
using System;
using Tenray.Topaz.Options;

namespace Tenray.Topaz
{
    public interface ITopazEngine
    {
        int Id { get; }

        TopazEngineOptions Options { get; set; }
        
        ITopazEngineScope GlobalScope { get; }

        /// <summary>
        /// Executes the script in the global scope.
        /// </summary>
        /// <param name="code"></param>
        void ExecuteScript(string code);

        /// <summary>
        /// Executes the single line expression in the global scope.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        object ExecuteExpression(string code);

        /// <summary>
        /// Executes the function that is declared in the global scope with given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object InvokeFunction(string name, params object[] args);

        void AddType<T>(
            string name = null,
            Action<CallType, object[]> argsConverter = null,
            VariableKind variableKind = VariableKind.Const,
            TypeOptions typeOptions = TypeOptions.Default);
        
        void AddType(
            Type type,
            string name = null,
            Action<CallType, object[]> argsConverter = null,
            VariableKind variableKind = VariableKind.Const,
            TypeOptions typeOptions = TypeOptions.Default);

        /// <summary>
        /// Gets the value of the variable that is defined in the global scope.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns></returns>
        object GetValue(string name);

        /// <summary>
        /// Defines a new variable with given name and value
        /// or updates existing variable with given name and value
        /// in the global scope.
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

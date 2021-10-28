using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Utility
{
    internal static class BindingHelper
    {
       internal static void BindVariables(
            ScriptExecutor scriptExecutor,
            object value,
            VariableDeclaration variableDeclaration)
        {
            var vars = variableDeclaration.Declarations;
            var len = vars.Count;
            if (len == 0)
                return;
            if (len == 1)
            {
                var identifier = (Identifier)vars[0].Id;
                scriptExecutor.AddOrUpdateVariableValueInTheScope(
                    identifier.Name, value, VariableKind.Let, true);
                return;
            }
        }
    }
}

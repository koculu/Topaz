using Esprima.Ast;

namespace Tenray.Topaz.Core
{
    internal class Variable
    {
        internal object Value;

        internal VariableKind Kind;

        internal VariableState State;

        internal bool ShouldCapture;

        internal Variable(object value,
            VariableKind kind,
            VariableState state = VariableState.None)
        {
            Value = value;
            Kind = kind;
            State = state;
            ShouldCapture = kind == VariableKind.Let;
        }
        
        internal void SetValueAndKind(
            object value, VariableKind kind, VariableState state)
        {
            Value = value;
            if (kind == Kind)
                return;
            State = state;
            Kind = kind;
            ShouldCapture = kind == VariableKind.Let;
        }

        internal void SetKind(VariableKind kind)
        {
            if (kind == Kind)
                return;
            Kind = kind;
            ShouldCapture = kind == VariableKind.Let;
        }

        internal bool IsConst => Kind == VariableKind.Const;

        internal bool IsLet => Kind == VariableKind.Let;

        internal bool IsVar => Kind == VariableKind.Var;

        internal bool IsCaptured => State == VariableState.Captured;
    }
}

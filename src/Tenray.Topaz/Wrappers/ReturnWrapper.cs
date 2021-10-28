using Esprima.Ast;

namespace Tenray.Topaz
{
    internal class ReturnWrapper
    {
        internal object Result { get; }

        internal ReturnWrapper(object result)
        {
            Result = result;
        }
    }
}

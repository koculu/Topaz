namespace Tenray.Topaz
{
    public sealed class Undefined
    {
        public static readonly Undefined Value = new ();
        private Undefined()
        {
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}

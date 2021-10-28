namespace Tenray.Topaz
{
    public class Undefined
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

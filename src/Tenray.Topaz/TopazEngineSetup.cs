using Tenray.Topaz.Interop;
using Tenray.Topaz.Options;

namespace Tenray.Topaz
{
    /// <summary>
    /// Initialization properties for TopazEngine constructor.
    /// If you don't set some property in the setup, TopazEngine will use default implementation.
    /// </summary>
    public class TopazEngineSetup
    {
        public bool IsThreadSafe { get; set; } = true;

        public TopazEngineOptions Options { get; set; }

        public IObjectProxyRegistry ObjectProxyRegistry { get; set; }

        public IObjectProxy DefaultObjectProxy { get; set; }

        public IDelegateInvoker DelegateInvoker { get; set; }

        public IMemberAccessPolicy MemberAccessPolicy { get; set; }

        public IValueConverter ValueConverter { get; set; }
    }
}

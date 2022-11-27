using System;

namespace Tenray.Topaz;

public sealed class TopazException : Exception
{
    public TopazException(string message) : base(message)
    {
    }
}

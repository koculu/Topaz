namespace Esprima;

/// <summary>
/// Default error handling logic for Esprima.
/// </summary>
public class ErrorHandler : IErrorHandler
{
    public string? Source { get; set; }
    public bool Tolerant { get; set; }

    public virtual void RecordError(ParserException error)
    {
    }

    public void Tolerate(ParserException error)
    {
        if (Tolerant)
        {
            RecordError(error);
        }
        else
        {
            throw error;
        }
    }

    public ParserException CreateError(int index, int line, int column, string message)
    {
        return new ParserException(new ParseError(message, Source, index, new Position(line, column)));
    }

    public void TolerateError(int index, int line, int column, string message)
    {
        var error = CreateError(index, line, column, message);
        if (Tolerant)
        {
            RecordError(error);
        }
        else
        {
            throw error;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Tenray.Topaz;

public interface ITopazFunction
{
    /// <summary>
    /// Gets name of the function.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the number of arguments of the function.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets the nth argument name of the function.
    /// </summary>
    /// <param name="index">The index of the argument.</param>
    /// <returns>Name of the argument.</returns>
    string this[int index] { get; }

    /// <summary>
    /// Invokes the function using arguments.
    /// </summary>    
    /// <param name="token">CancellationToken.</param>
    /// <param name="args">Arguments.</param>
    /// <returns>The object returned from function implementation.</returns>
    object Invoke(CancellationToken token, params object[] args);

    /// <summary>
    /// Invokes the function asynchronously using arguments.
    /// </summary>
    /// <param name="args">Arguments.</param>
    /// <param name="token">CancellationToken.</param>
    /// <returns>The object returned from function implementation.</returns>
    ValueTask<object> InvokeAsync(CancellationToken token, params object[] args);
}
namespace Backdash.Synchronizing.Input;

using System;

/// <summary>
///     Input value provider
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IInputGenerator<out TInput> where TInput : unmanaged
{
    /// <summary>
    ///     Returns the next input
    /// </summary>
    TInput Next();
}

/// <summary>
///     Random input value provider
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <remarks>
///     Initializes new <see cref="RandomInputGenerator{TInput}" />
/// </remarks>
public sealed class RandomInputGenerator<TInput>(Random? random = null)
    : IInputGenerator<TInput> where TInput : unmanaged
{
    Random Random { get; } = random ?? Random.Shared;

    /// <inheritdoc />
    public TInput Next() => Random.Generate<TInput>();
}

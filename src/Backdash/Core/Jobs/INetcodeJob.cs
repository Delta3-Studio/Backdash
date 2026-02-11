namespace Backdash.Core;

/// <summary>
/// Defines an asynchronous background job
/// </summary>
public interface INetcodeJob
{
    /// <summary>
    /// Job name identity
    /// </summary>
    string? JobName { get; }

    /// <summary>
    /// Job task
    /// </summary>
    Task Start(CancellationToken cancellationToken);
}

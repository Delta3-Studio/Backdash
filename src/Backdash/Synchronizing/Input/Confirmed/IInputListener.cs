namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///     Listener for confirmed inputs
/// </summary>
public interface IInputListener<TInput> : IDisposable where TInput : unmanaged
{
    /// <summary>
    ///     Session Started
    /// </summary>
    void OnSessionStart(InputContext<TInput> context);

    /// <summary>
    ///     New confirmed input event handler
    /// </summary>
    void OnConfirmed(in Frame frame, in ConfirmedInputs<TInput> inputs);

    /// <summary>
    ///     Session End
    /// </summary>
    void OnSessionClose();
}

using System.Diagnostics.CodeAnalysis;

namespace Backdash.Synchronizing.State;

/// <summary>
///     Repository for temporary save and restore game states.
/// </summary>
public interface IStateStore
{
    /// <summary>
    ///     Initialize the state buffer with capacity of <paramref name="saveCount" />
    /// </summary>
    /// <param name="saveCount"></param>
    void Initialize(int saveCount);

    /// <summary>
    ///     Try to load the <see cref="SavedState" /> for <paramref name="frame" />.
    /// </summary>
    /// <returns>true if the frame was found, false otherwise</returns>
    bool TryLoad(Frame frame, [MaybeNullWhen(false)] out SavedState result);

    /// <summary>
    ///     Try to read the <see cref="SavedState" /> for <paramref name="frame" />.
    /// </summary>
    /// <returns>true if the frame was found, false otherwise</returns>
    bool TryGet(Frame frame, [MaybeNullWhen(false)] out SavedState result);

    /// <summary>
    ///     Returns last <see cref="SavedState" />.
    /// </summary>
    SavedState Last();

    /// <summary>
    ///     Returns next writable <see cref="SavedState" />.
    /// </summary>
    ref SavedState Next();

    /// <summary>
    ///     Advance the store pointer
    /// </summary>
    void Advance();

    /// <summary>
    ///     Return a <see cref="SavedState" /> for <paramref name="frame" /> if exists, <c>null</c> otherwise.
    /// </summary>
    SavedState? Get(Frame frame) => TryGet(frame, out var savedState) ? savedState : null;
}

using Backdash;
using Backdash.Synchronizing.Input.Confirmed;
using SpaceWar.Logic;

namespace SpaceWar;

/// <summary>
/// Sample of an implementation for saving inputs.
/// </summary>
sealed class InputsFileListener(string filename) : IInputListener<PlayerInputs>
{
    readonly FileStream fileStream = File.Create(filename);

    byte[] inputBuffer = [];
    InputContext<PlayerInputs> inputContext = null!;

    public void OnSessionStart(InputContext<PlayerInputs> context)
    {
        inputBuffer = new byte[context.ConfirmedInputSize];
        inputContext = context;

        fileStream.SetLength(0);
        fileStream.Seek(0, SeekOrigin.Begin);
    }

    public void OnSessionClose() => fileStream.Flush();

    public void OnConfirmed(in Frame frame, in ConfirmedInputs<PlayerInputs> inputs)
    {
        var written = inputContext.Write(inputBuffer, in inputs);
        fileStream.Write(inputBuffer, 0, written);

        var paddingCount = inputContext.ConfirmedInputSize - written;
        for (var i = 0; i < paddingCount; i++)
            fileStream.WriteByte(0);

    }

    public void Dispose() => fileStream.Dispose();
}

sealed class InputsFileProvider(string file) : IInputProvider<PlayerInputs>
{
    public IReadOnlyList<ConfirmedInputs<PlayerInputs>> GetInputs(InputContext<PlayerInputs> context)
    {
        if (!File.Exists(file))
            throw new InvalidOperationException("Invalid replay file");

        var buffer = new byte[context.ConfirmedInputSize];
        List<ConfirmedInputs<PlayerInputs>> result = [];
        ConfirmedInputs<PlayerInputs> confirmedInput = new();

        using var replayStream = File.OpenRead(file);
        while (replayStream.Read(buffer) > 0)
        {
            context.Read(buffer, ref confirmedInput);
            result.Add(confirmedInput);
        }

        return result.AsReadOnly();
    }
}

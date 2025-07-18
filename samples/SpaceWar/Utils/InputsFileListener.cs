using System.IO.Compression;
using Backdash;
using Backdash.Network;
using Backdash.Synchronizing.Input.Confirmed;
using SpaceWar.Logic;

namespace SpaceWar;

/// <summary>
/// Sample of an implementation for listening inputs.
/// </summary>
sealed class InputsFileListener(string fileName) : IInputListener<PlayerInputs>
{
    readonly FileStream fileStream = File.Create(NetUtils.GetTempFile());

    byte[] inputBuffer = [];
    InputContext<PlayerInputs> inputContext = null!;

    public void OnSessionStart(InputContext<PlayerInputs> context)
    {
        inputBuffer = new byte[context.ConfirmedInputSize];
        inputContext = context;

        fileStream.SetLength(0);
        fileStream.Seek(0, SeekOrigin.Begin);
    }

    public void OnSessionClose()
    {
        fileStream.Flush();
        CompressFile();
    }

    public void OnConfirmed(in Frame frame, in ConfirmedInputs<PlayerInputs> inputs)
    {
        var written = inputContext.Write(inputBuffer, in inputs);
        fileStream.Write(inputBuffer, 0, written);

        var paddingCount = inputContext.ConfirmedInputSize - written;
        for (var i = 0; i < paddingCount; i++)
            fileStream.WriteByte(0);
    }

    public void Dispose() => fileStream.Dispose();

    void CompressFile()
    {
        using var compressed = File.Create(fileName);
        using DeflateStream compressor = new(compressed, CompressionMode.Compress);
        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.CopyTo(compressor);
    }
}

sealed class InputsFileProvider(string fileName) : IInputProvider<PlayerInputs>
{
    public IReadOnlyList<ConfirmedInputs<PlayerInputs>> GetInputs(InputContext<PlayerInputs> context)
    {
        if (!File.Exists(fileName))
            throw new InvalidOperationException("Invalid replay file");

        var buffer = new byte[context.ConfirmedInputSize];
        List<ConfirmedInputs<PlayerInputs>> result = [];
        ConfirmedInputs<PlayerInputs> confirmedInput = new();

        using var replayStream = DecompressFile(fileName);
        while (replayStream.Read(buffer) > 0)
        {
            context.Read(buffer, ref confirmedInput);
            result.Add(confirmedInput);
        }

        return result.AsReadOnly();
    }

    static FileStream DecompressFile(string fileName)
    {
        using var compressedFileStream = File.OpenRead(fileName);
        var outputFileStream = File.Create(NetUtils.GetTempFile());
        using DeflateStream decompressor = new(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(outputFileStream);
        outputFileStream.Seek(0, SeekOrigin.Begin);
        return outputFileStream;
    }
}

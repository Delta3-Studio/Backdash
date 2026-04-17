using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Core;
using Backdash.Serialization;

namespace Backdash.Synchronizing.State;

/// <summary>
///     Get string representation of the state
///     Used for Sync Test logging <see cref="NetcodeSessionBuilder{TInput}.ForSyncTest" />
/// </summary>
public interface IStateStringParser
{
    /// <summary>
    ///     Parse binary state to a string representation.
    /// </summary>
    string GetStateString(in Frame frame, ref readonly BinaryBufferReader reader, object? currentState);
}

/// <inheritdoc />
sealed class DefaultStateStringParser : IStateStringParser
{
    /// <inheritdoc />
    public string GetStateString(in Frame frame, ref readonly BinaryBufferReader reader, object? currentState) =>
        currentState?.ToString() ?? string.Empty;
}

/// <inheritdoc />
sealed class HexStateStringParser : IStateStringParser
{
    /// <inheritdoc />
    public string GetStateString(in Frame frame, ref readonly BinaryBufferReader reader, object? currentState) =>
        $$"""
          {
            --- BEGIN State-Hex ---
            {{Convert.ToHexString(reader.CurrentBuffer)}}
            --- END State-Hex  ---
          }
          """;
}

/// <summary>
///     Try to get the JSON string representation of the state.
/// </summary>
public sealed class JsonStateStringParser(
    JsonSerializerOptions? options = null,
    IStateStringParser? stateStringFallback = null
) : IStateStringParser
{
    static readonly Lazy<IStateStringParser> singleton = new(new JsonStateStringParser());

    /// <summary>Singleton instance</summary>
    public static IStateStringParser Singleton => singleton.Value;

    static JsonSerializerOptions CreateDefaultJsonOptions(Action<JsonSerializerOptions>? configure = null)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        configure?.Invoke(options);
        return options;
    }

    internal Logger? Logger = null;
    readonly JsonSerializerOptions jsonOptions = options ?? CreateDefaultJsonOptions();
    readonly IStateStringParser fallback = stateStringFallback ?? new DefaultStateStringParser();
    readonly HexStateStringParser nullFallback = new();

    /// <summary>
    /// Create and configure a JSON <see cref="IStateStringParser"/>
    /// </summary>
    public JsonStateStringParser(Action<JsonSerializerOptions> configure,
        IStateStringParser? stateStringFallback = null) :
        this(CreateDefaultJsonOptions(configure), stateStringFallback)
    { }

    /// <inheritdoc />
    public string GetStateString(in Frame frame, ref readonly BinaryBufferReader reader, object? currentState)
    {
        if (currentState is null)
            return nullFallback.GetStateString(in frame, in reader, currentState);

        try
        {
            return JsonSerializer.Serialize(currentState, jsonOptions);
        }
        catch (Exception e)
        {
            Logger?.Write(LogLevel.Error, $"State Json Parser Error: {e}");
            return fallback.GetStateString(in frame, in reader, currentState);
        }
    }
}

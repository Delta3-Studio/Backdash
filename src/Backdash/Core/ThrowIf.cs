using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backdash.Core;

static class ThrowIf
{
    [DebuggerStepThrough]
    public static void NotInRange(
        int argument,
        int min = int.MinValue,
        int max = int.MaxValue,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument < min || argument > max)
            throw new ArgumentOutOfRangeException(paramName, argument,
                $"Value must be between {min} and {max}.");
    }

    [DebuggerStepThrough]
    public static void NotDefined<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(paramName,
                $"Invalid enum value for type {typeof(T).Name}: '{value}'");
    }

    [DebuggerStepThrough]
    public static void NotDefined<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : struct, Enum
    {
        if (value is { } enumValue)
            NotDefined(enumValue, paramName);
    }

    [DebuggerStepThrough]
    public static void Negative(
        TimeSpan value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) => ArgumentOutOfRangeException.ThrowIfNegative(value.Ticks, paramName);

    [DebuggerStepThrough]
    public static void Negative<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : struct, INumberBase<T>
        => ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);

    [DebuggerStepThrough]
    public static void NotPositive(
        TimeSpan value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
    }

    [DebuggerStepThrough]
    public static void NotPositive<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : struct, INumberBase<T>
        => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);

    [Conditional("DEBUG")]
    [Conditional("TRACE")]
    [Conditional("TOOLS")]
    [DebuggerStepThrough]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        string? info = null,
        [CallerArgumentExpression(nameof(condition))]
        string? paramName = null,
        [CallerFilePath] string? location = null,
        [CallerLineNumber] int line = 0
    )
    {
        if (!condition)
            throw new NetcodeAssertionException(
                $"False assertion {paramName}: {info ?? "expected true, got false"} in {location}:{line}");
    }
}

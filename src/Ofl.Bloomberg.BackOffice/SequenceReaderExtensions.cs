using Ofl.Bloomberg.Parsing;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ofl.Bloomberg.BackOffice;

public static class SequenceReaderExtensions
{
    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidOperationException CreateCouldNotFindFieldValueDelimiterException() =>
        new(
            $"Could not find delimiter '{(char)Constant.FieldValueDelimiter}'"
        );

    #endregion

    #region Core extensions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<byte> ReadNextBloombergFieldValue(
        this ref SequenceReader<byte> reader
    ) => reader.TryReadTo(
        out ReadOnlySequence<byte> sequence
        , Constant.FieldValueDelimiter
    )
        ? sequence
        : throw CreateCouldNotFindFieldValueDelimiterException();

    #endregion

    #region Type parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadNextBloombergFieldValueAsIntegerInt32(
        this ref SequenceReader<byte> reader
        , out int value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Get the next field value.
        var field = reader.ReadNextBloombergFieldValue();

        // Try and read.
        return field.TryParseIntegerInt32(out value, parseDetail, out detail);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadNextBloombergFieldValueAsIntegerInt32(
        this ref SequenceReader<byte> reader
        , out int value
        , out ParsingDetail detail
    ) => reader.TryReadNextBloombergFieldValueAsIntegerInt32(
        out value
        , true
        , out detail
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadNextBloombergFieldValueAsIntegerInt32(
        this ref SequenceReader<byte> reader
        , out int value
    ) => reader.TryReadNextBloombergFieldValueAsIntegerInt32(
        out value
        , false
        , out _
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadNextBloombergFieldValueAsIntegerInt32(
        this ref SequenceReader<byte> reader
    ) => reader.TryReadNextBloombergFieldValueAsIntegerInt32(
            out int value
            , out ParsingDetail detail
        )
        ? value
        : throw new InvalidOperationException(
            $"Could not parse an {nameof(Int32)} value from the {nameof(reader)}.  "
            + $"Call to {nameof(TryReadNextBloombergFieldValueAsIntegerInt32)} with a value of" 
            + $"\"{Constant.Encoding.GetString(reader.CurrentSpan)}\" returned a {nameof(ParsingDetail)} of {detail}."
        );

    #endregion
}

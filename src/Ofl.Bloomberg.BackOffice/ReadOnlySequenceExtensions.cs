using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ofl.Bloomberg.BackOffice;

public static class ReadOnlySequenceExtensions
{
    #region Extensions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DecodeString(
        this in ReadOnlySequence<byte> buffer
    ) => buffer.IsEmpty ? string.Empty : Constant.Encoding.GetString(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<byte> ReadToDelimiter(
        this ref ReadOnlySequence<byte> buffer
        , in byte delimiter
    ) => buffer
        .TryReadToDelimiter(delimiter)
        ?? throw new InvalidOperationException(
            $"Could not find delimiter '{(char) delimiter}'"
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<byte>? TryReadToDelimiter(
        this ref ReadOnlySequence<byte> buffer
        , in byte delimiter
    )
    {
        // The position.
        SequencePosition? position;

        // The line.
        ReadOnlySequence<byte>? line = default;

        // Look for a new line in the buffer
        position = buffer.PositionOf(delimiter);

        // If there is a position, then 
        if (position != null)
        {
            // Set the line.
            line = buffer.Slice(0, position.Value);

            // Skip the line + the new line character (basically position)
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        }

        // Return the line.
        return line;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<byte>? TryReadLine(
        this ref ReadOnlySequence<byte> buffer
    ) => buffer.TryReadToDelimiter(Constant.NewLine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SequenceEqual<T>(
        this in ReadOnlySequence<T> sequence
        , ReadOnlySpan<T> span
    )
        where T : unmanaged, IEquatable<T>
    {
        // If there is one segment, compare.
        if (sequence.IsSingleSegment)
            return sequence.FirstSpan.SequenceEqual(span);

        // Quick shot, if the length is not equal, then
        // bail.
        if (sequence.Length != span.Length)
            return false;

        // The sequence position.  Start at the beginning of the sequence.
        var next = sequence.Start;

        // Try and get the next memory item, advance the sequence position.
        while (sequence.TryGet(ref next, out ReadOnlyMemory<T> nextSegment, true))
        {
            // If there is length, then great, try to compare, otherwise, skip
            // as it's pointless.
            if (nextSegment.Length > 0)
            {
                // Cool, we got something.  Get the span.
                ReadOnlySpan<T> nextSpan = nextSegment.Span;

                // If the span is greater in length to the span, then
                // return false.
                if (nextSpan.Length > span.Length)
                    return false;

                // Ok, they are at least equal, get a local span for comparison.
                var localSpan = span[..nextSpan.Length];

                // Compare.  If not equal, return false.
                if (!nextSpan.SequenceEqual(localSpan))
                    return false;
                
                // They are equal.  Slice the span to start after the next span
                // ends.
                span = span[nextSpan.Length..];
            }
        }

        // Everything is equal.
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsProcessableLine(
        this in ReadOnlySequence<byte> line
    ) => !line.IsEmpty
        && line.Slice(line.GetPosition(0)).FirstSpan[0] != Constant.Comment;


    public static bool TryParseKeyValue(
        this in ReadOnlySequence<byte> line
        , out ReadOnlySequence<byte> key
        , out ReadOnlySequence<byte> value
    )
    {
        // Set defaults.
        key = default;
        value = default;

        // If not processable, bail.
        if (!line.IsProcessableLine()) return false;

        // The position of the key value delimiter.
        var keyValueDelimiterPosition = line
            .PositionOf(Constant.KeyValueDelimiter);

        // If the position is null, return false.
        if (keyValueDelimiterPosition is null) return false;

        // Split the two.
        key = line.Slice(0, keyValueDelimiterPosition.Value);

        // Need to slice value twice, once to get the equals
        // and the rest, then one to trim that off.
        value = line.Slice(keyValueDelimiterPosition.Value, line.End);
        value = value.Slice(1);

        // This was parsed, bail.
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureSequenceEqual(
        this ReadOnlySequence<byte> left
        , in ReadOnlySpan<byte> right
    )
    {
        // If not start of data, bail.
        if (!left.SequenceEqual(right))
            throw new InvalidOperationException(
                $"Expected to read sequence \"{right.DecodeString()}\" but "
                + $"read \"{left.DecodeString()}\"."
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Parse<T>(
        this in ReadOnlySequence<byte> sequence
        , IFormatProvider? formatProvider = default
    )
        where T : ISpanParsable<T>
    {
        // If there is one span of bytes, then use a fast track using just that
        // span.
        if (sequence.IsSingleSegment)
            return sequence.FirstSpan.Parse<T>(formatProvider);

        // We need to copy to a span of bytes.
        // If the length of the sequence is too large for
        // the stack, we have other problems.
        Span<byte> bytes = stackalloc byte[(int) sequence.Length];

        // Copy to the bytes.
        sequence.CopyTo(bytes);

        // Get a read only span.
        ReadOnlySpan<byte> roBytes = bytes;

        // Call the parse method.
        return roBytes.Parse<T>(formatProvider);
    }

    #endregion
}

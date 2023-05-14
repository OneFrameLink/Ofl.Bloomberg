using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Extensions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task IgnoreStartOfFileAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , in ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , CancellationToken cancellationToken = default
    ) => bloombergBackOfficeReader.ReadStartOfFileAsync(
        doNotAdvanceIfLineEquals
        , Constant.IgnoreLineProcessing
        , cancellationToken
    );

    // TODO: Could potentially add "in" to doNotAdvanceIfLineEquals
    // If return of dictionary is in a continuation and avoids await/async
    public static async Task<ReadOnlyDictionary<string, string>> ReadStartOfFileAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , CancellationToken cancellationToken = default
    )
    {
        // The results.
        var results = new Dictionary<string, string>();

        // Splits the line into two sequences
        // and passes them to the key/value operation.
        void Accumulate(
            in ReadOnlySequence<byte> keyBytes
            , in ReadOnlySequence<byte> valueBytes
        )
        {
            // Get the key and value strings.
            var key = keyBytes.DecodeString();
            var value = valueBytes.DecodeString();

            // Add to the dictionary.
            results.Add(key, value);
        }

        // Call the overload.
        await bloombergBackOfficeReader.ReadStartOfFileAsync(
            doNotAdvanceIfLineEquals
            , Accumulate
            , cancellationToken
        )
        .ConfigureAwait(false);

        // Return the dictionary.
        return new ReadOnlyDictionary<string, string>(results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ReadStartOfFileAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , in ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , KeyValueAccumulator accumulator
        , CancellationToken cancellationToken = default
    )
    {
        // Splits the line into two sequences
        // and passes them to the key/value operation.
        void Accumulate(in ReadOnlySequence<byte> line)
        {
            // Try and parse the key value pair.
            if (line.TryParseKeyValue(out var key, out var value))
                // Call the operation.
                accumulator(key, value);
        }

        // Call the overload.
        return bloombergBackOfficeReader.ReadStartOfFileAsync(
            doNotAdvanceIfLineEquals
            , Accumulate
            , cancellationToken
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ReadStartOfFileAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , in ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , LineAccumulator accumulator
        , CancellationToken cancellationToken = default
    ) => bloombergBackOfficeReader.AccumulateSectionAsync(
        BloombergBackOfficeSection.StartOfFile
        , doNotAdvanceIfLineEquals
        , false
        , accumulator
        , cancellationToken
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ReadEndOfFileAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , CancellationToken cancellationToken = default
    )
    {
        // Accumulator.
        static void Accumulate(in ReadOnlySequence<byte> line)
        {
            // If this is a valid line, throw.
            if (line.IsProcessableLine())
                throw new InvalidOperationException(
                    "Encountered a processable line while trying to read to "
                    + $"\"{line.DecodeString()}\"."
                );
        }

        // Call the overload.
        return bloombergBackOfficeReader.AccumulateSectionAsync(
            default
            , BloombergBackOfficeSection.EndOfFile
            , true
            , Accumulate
            , cancellationToken
        );
    }

    #endregion
}

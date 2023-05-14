using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task IgnoreKeyValuePairsUntilAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , in ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , CancellationToken cancellationToken = default
    ) => bloombergBackOfficeReader.ReadKeyValuePairsUntilAsync(
        doNotAdvanceIfLineEquals
        , Constant.IgnoreLineProcessing
        , cancellationToken
    );

    // TODO: Could potentially add "in" to doNotAdvanceIfLineEquals
    // If return of dictionary is in a continuation and avoids await/async
    public static async Task<ReadOnlyDictionary<string, string>> ReadKeyValuePairsUntilAsync(
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
        await bloombergBackOfficeReader.ReadKeyValuePairsUntilAsync(
            doNotAdvanceIfLineEquals
            , Accumulate
            , cancellationToken
        )
        .ConfigureAwait(false);

        // Return the dictionary.
        return new ReadOnlyDictionary<string, string>(results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ReadKeyValuePairsUntilAsync(
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
        return bloombergBackOfficeReader.ReadKeyValuePairsUntilAsync(
            doNotAdvanceIfLineEquals
            , Accumulate
            , cancellationToken
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ReadKeyValuePairsUntilAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , in ReadOnlyMemory<byte> until
        , LineAccumulator accumulator
        , CancellationToken cancellationToken = default
    ) => bloombergBackOfficeReader.AccumulateSectionAsync(
        default
        , until
        , false
        , accumulator
        , cancellationToken
    );

    #endregion
}

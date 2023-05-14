using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Extensions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task IgnoreFieldsAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , CancellationToken cancellationToken = default
    ) => bloombergBackOfficeReader.ReadFieldsAsync(
        Constant.IgnoreLineProcessing
        , cancellationToken
    );

    public static async Task<ReadOnlyCollection<string>> ReadFieldsAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , CancellationToken cancellationToken = default
    )
    {
        // The list of fields.
        var results = new List<string>();

        // Accumulator.
        void Accumulate(in ReadOnlySequence<byte> line)
        {
            // If the line is not processable, bail.
            if (!line.IsProcessableLine())
                return;

            // Convert to a string and add.
            results.Add(line.DecodeString());
        }

        // Call the overload.
        await bloombergBackOfficeReader.ReadFieldsAsync(
            Accumulate
            , cancellationToken
        )
        .ConfigureAwait(false);

        // Wrap and return.
        return new ReadOnlyCollection<string>(results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task ReadFieldsAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , LineAccumulator accumulator
        , CancellationToken cancellationToken = default
    ) => await bloombergBackOfficeReader.AccumulateSectionAsync(
        BloombergBackOfficeSection.StartOfFields
        , BloombergBackOfficeSection.EndOfFields
        , true
        , accumulator
        , cancellationToken
    )
    .ConfigureAwait(false);

    #endregion
}

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Extensions

    public static async ValueTask IgnoreDataSectionAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , CancellationToken cancellationToken = default
    )
    {
        // Read the next line.
        ReadOnlySequence<byte> line = await bloombergBackOfficeReader
            .ReadLineAsync(cancellationToken)
            .ConfigureAwait(false);

        // If not start of data, bail.
        line.EnsureSequenceEqual(BloombergBackOfficeSection.StartOfData);

        // Read lines until there is an end of data section.
        while (true)
        {
            // Read the line.
            line = await bloombergBackOfficeReader
                .ReadLineAsync(cancellationToken)
                .ConfigureAwait(false);

            // If end of data section, bail.
            if (line.SequenceEqual(BloombergBackOfficeSection.EndOfData))
                return;

            // If not a valid line, continue.
            if (!line.IsProcessableLine()) continue;
        }
    }

    // NOTE: There is a manual implementation of an enumerator here:
    // https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/commits/9a4c05ed1e5ecba1121068669b7bccf04a2d9d16#Ofl.Bloomberg.BackOffice/src/Ofl.Bloomberg.BackOffice/BloombergBackOfficeDataSectionEnumerator.cs
    // However, when benchmarked against the credit risk dif file:
    // https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/commits/44750f05c6d0e1b0a5422bbfaa41c545337c0edf#Ofl.Bloomberg.BackOffice/benchmarks/Ofl.Bloomberg.BackOffice.Benchmarks/EnumerateDataSectionBenchmark.cs
    // The implementation about was the same in speed (both about 71 ms, custom was longer in some cases with higher an error rate of about 3ms and a mean of 73ms)
    // and a memory use of 3100kb vs the 650kb from below.
    // So for now, we keep this.
    public static async IAsyncEnumerable<T> EnumerateDataSection<T>(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        // Feels like we might need a parameters object at some point.
        , [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) where T : class, IReusableBloombergBackOfficeRow<T>
    {
        // Read the next line.
        ReadOnlySequence<byte> line = await bloombergBackOfficeReader
            .ReadLineAsync(cancellationToken)
            .ConfigureAwait(false);

        // If not start of data, bail.
        line.EnsureSequenceEqual(BloombergBackOfficeSection.StartOfData);

        // The array pool.
        // TODO: Consider memory pools other than the shared?
        var pool = ArrayPool<ReadOnlySequence<byte>>.Shared;

        // Create the parameters.
        var parameters = new ReusableBloombergBackOfficeRowParameters(pool);

        // The row.
        T? row = default;

        // Wrap in a try/finally, have to dispose.
        try
        {
            // Read lines until there is an end of data section.
            while (true)
            {
                // Read the line.
                line = await bloombergBackOfficeReader
                    .ReadLineAsync(cancellationToken)
                    .ConfigureAwait(false);

                // If end of data section, bail.
                if (line.SequenceEqual(BloombergBackOfficeSection.EndOfData))
                    yield break;

                // If not a valid line, continue.
                if (!line.IsProcessableLine()) continue;

                // Create the row if not created.
                // In looking to see if separating into two loops would be more performant (to avoid a branch with the ??= operator):
                //
                // https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/src/Ofl.Bloomberg.BackOffice/BloombergBackOfficeReaderExtensions.EnumerateDataSection.cs?at=dd604b615ae8fd5d76b511f7947efda0b888a50f#38
                //
                // The results were underwhelming:
                //
                // |                    Method |     Mean |    Error |   StdDev | Allocated |
                // |-------------------------- |---------:|---------:|---------:|----------:|
                // | FileStreamAsyncForEachOld | 68.69 ms | 1.794 ms | 5.147 ms | 688.19 KB |
                // | FileStreamAsyncForEachNew | 68.74 ms | 1.359 ms | 2.485 ms | 688.26 KB |
                //
                // Also note that in looking at memory consumpion in dotMemory, the allocated is the result of the pipe stream
                // and not much in this, or other methods.
                row ??= T.Create(parameters);

                // Update/reuse.
                T.Reuse(row, line);

                // Yield.
                yield return row;
            }
        }
        finally
        {
            // Dispose of the row.
            using (row) { }
        }
    }

    #endregion
}

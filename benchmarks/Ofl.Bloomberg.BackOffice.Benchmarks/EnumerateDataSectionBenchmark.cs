using BenchmarkDotNet.Attributes;

namespace Ofl.Bloomberg.BackOffice.Benchmarks;

[MemoryDiagnoser]
public class EnumerateDataSectionBenchmark
{
    #region Instance state

    private BloombergBackOfficeReader _reader = default!;

    #endregion

    #region Parameters

    [Params(
        "credit_risk.dif"
        , "MSG1Parsing1600Muni.out"

        // NOTE: This is a large file, taking about 30 seconds per iteration
        // , "credit_risk.out"
        // NOTE: This is also fairly large file, taking about 4 seconds per iteration.
        // , "equityOptionsExtendedEuro1V2.px"
    )]
    public string FileToProcess { get; set; } = default!;

    #endregion

    #region Setup/teardown

    [IterationSetup]
    public void SetupIteration()
    {
        // Load the credit risk file.
        var stream = File.OpenRead(
            Path.Combine(
                // TODO: Find a way to make relative.
                "C:\\d2\\Ofl.Bloomberg\\files"
                , FileToProcess
            )
        );

        // Create a reader.
        _reader = new(stream);

        // Async part.
        async Task IterationSetupAsync()
        {
            // Read start of file without headers.
            await _reader.IgnoreStartOfFileAsync(
                BloombergBackOfficeSection.StartOfFields
                , default
            );

            // Read the fields.
            await _reader
                .IgnoreFieldsAsync(default)
                .ConfigureAwait(false);

            // Get the headers.
            await _reader.IgnoreKeyValuePairsUntilAsync(
                BloombergBackOfficeSection.StartOfData
                , default
            )
            .ConfigureAwait(false);
        }

        // Run and wait.
        Nito.AsyncEx.AsyncContext.Run(() => IterationSetupAsync());
    }

    [IterationCleanup]
    public void CleanupIteration()
    {
        // Dispose of the reader.
        _reader.Dispose();
    }

    #endregion

    #region Benchmarks

    [Benchmark]
    public async Task<int> FileStreamAsyncForEach()
    {
        // The count.
        var count = 0;

        // Get the count.
        await foreach (var _ in _reader.EnumerateDataSection<BloombergBackOfficeDescriptiveDataFileRow>())
            count++;

        // Return the count.
        return count;
    }

    #endregion
}

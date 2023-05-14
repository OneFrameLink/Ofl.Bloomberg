using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;

namespace Ofl.Data.SqlClient.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(Config))]
public class SqlBulkCopyMapperColumnMappingExtensionsBenchmarks
{
    #region Configuration

    public class Config : ManualConfig
    {
        #region Constructor

        public Config()
        {
            // Create the CSV exporter.
            var exporter = new CsvExporter(
                CsvSeparator.CurrentCulture,
                new SummaryStyle(
                    cultureInfo: System.Globalization.CultureInfo.CurrentCulture,
                    printUnitsInHeader: true,
                    printUnitsInContent: false,
                    timeUnit: Perfolizer.Horology.TimeUnit.Nanosecond,
                    sizeUnit: SizeUnit.KB
                ));

            // Add.
            AddExporter(exporter);
        }

        #endregion
    }

    #endregion

    #region Instance state

    private ISqlBulkCopyRowMapperRunner _runner = default!;

    private int[] _indicesToAccess = default!;

    #endregion

    #region Parameters

    [Params(
        true
        , false
    )]
    public bool SequentialAccess { get; set; }

    [Params(
        true
        , false
    )]
    public bool FromDelegate { get; set; }

    [Params(
        //1
        //, 10
        //, 100
        1_000
        //, 10_000
        //, 100_000
        //, 1_000_000
        //, 10_000_000
    )]
    public int Columns { get; set; } = default!;

    [Params(
        //0
        //, 1
        //, 10
        //, 100
        1_000
        //, 10_000
    )]
    public int Offset { get; set; }

    [Params(
        //1
        //, 10
        //, 100
        1_000
        //, 10_000
    )]
    public int Step { get; set; }

    [Params(
        typeof(object)
        , typeof(ValueTuple<int, int>)
    )]
    public Type InputType { get; set; } = default!;

    #endregion

    #region Helpers

    private int[] CreateIndicesToAcccess()
    {
        // Creates stepped mappings.
        IEnumerable<int> CreateSteppedMappings()
        {
            // The last value.
            var last = Offset;

            // Cycle through the count, increment
            // with offset.
            for (int i = 0; i < Columns; ++i)
            {
                // Yield the last.
                yield return last;

                // Increment by step.
                last += Step;
            }
        }

        // Get the mappings.
        var mappings = CreateSteppedMappings();

        // Put them in an array.
        var array = mappings.ToArray();

        // If randomizing them, do that here.
        if (!SequentialAccess)
        {
            // Create a random.
            var random = Random.Shared;

            // Shuffle.
            for (int i = 0; i < array.Length; ++i)
            {
                // Store what's at i in temp.
                var temp = array[i];

                // Get the random index.
                var randomIndex = random.Next(array.Length);

                // Put random index in i and temp in random index.
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }            
        }

        // Return the array.
        return array;
    }

    #endregion

    #region Setup/teardown

    [GlobalSetup]
    public void SetupGlobal()
    {
        // Get the indices to access.
        _indicesToAccess = CreateIndicesToAcccess();

        // Create the type
        var runnerType = typeof(SqlBulkCopyRowMapperRunner<>).MakeGenericType(InputType);

        // Create the runner.
        _runner = (ISqlBulkCopyRowMapperRunner) Activator.CreateInstance(
            runnerType
            , _indicesToAccess
            , FromDelegate
        )!;
    }

    [GlobalCleanup]
    public void CleanupGlobal()
    {
        // Dispose.
        using var _ = _runner;
    }

    #endregion

    #region Benchmarks

    [Benchmark(Baseline = true)]
    public object? Initial_Implementation()
    {
        // The value.
        object? lastValue = default;

        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
        {
            // Get the index.
            var index = _indicesToAccess[c];

            // Get the value.
            lastValue = _runner.Map(index);
        }

        // Return the value.
        return lastValue;
    }

    #endregion
}

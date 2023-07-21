using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using FastMember;

namespace Ofl.Data.SqlClient.Benchmarks;

[MemoryDiagnoser]
public class MapperBenchmarks<TInput, T>
    where T : new()
    where TInput : IInputType<T>, new()
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

    #region State

    private readonly TInput[] _rows = new TInput[1_000_000];

    private readonly ISqlBulkCopyRowMapper<TInput> _mapper = Enumerable
        .Range(0, 1_000)
        .Aggregate(
            new List<SqlBulkCopyMapperColumnMapping>(1_000)
            , (l, i) => {
                var mapping = SqlBulkCopyMapperColumnMapping.FromInstanceProperty(
                    i
                    , typeof(TInput)
                    , $"Value{i + 1}"
                );

                // Add to the list.
                l.Add(mapping);

                // Return the list.
                return l;
            }
            , l => l.CreateSqlBulkCopyMapper<TInput>()
        );

    private readonly string[] _memberNames = Enumerable
        .Range(0, 1_000)
        .Select(i => $"Value{i + 1}")
        .ToArray();

    // FastMember
    private readonly TypeAccessor _fastMember = TypeAccessor.Create(typeof(TInput));

    #endregion

    #region Parameters

    [Params(
        1
        //, 10
        //, 100
        //, 1_000
        //, 10_000
        //, 100_000
        //, 1_000_000
    )]
    public int Rows { get; set; }

    [Params(
        1
        , 10
        , 100
        , 1_000
    )]
    public int Columns { get; set; }

    #endregion

    #region Setup/teardown

    [GlobalSetup]
    public void SetupGlobal()
    {
        // Create the input.
        var input = new TInput();

        // The value.
        var values = new object[] { new T() };

        // Use reflection to set the values.
        for (int i = 0; i < 1000; i++)
        {
            // Get the property.
            var property = input
                .GetType()
                .GetProperty($"Value{i + 1}")!
                .GetSetMethod()!;

            // Call the method.
            property.Invoke(input, values);
        }

        // Set the row values.
        for (int i = 0; i < _rows.Length; i++)
            _rows[i] = input;
    }

    // BDN does not seem to support async here.
    [GlobalCleanup]
    public void CleanupIteration()
    {
        // The implementation.
        using var _ = _mapper;
    }

    #endregion

    #region Benchmarks

    [Benchmark(Baseline = true)]
    public object? Ofl()
    {
        // The object.
        object? value = default;

        // Cycle through rows.
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
            {
                // Get the value from input.
                var input = _rows[r];

                // Get the column value.
                value = _mapper.Map(in input, c);
            }

        // Return the value.
        return value;
    }

    [Benchmark]
    public object? FastMember()
    {
        // The object.
        object? value = default;

        // Cycle through rows.
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
            {
                // Get the value from input.
                var input = _rows[r];

                // Get the column value.
                value = _fastMember[input, _memberNames[c]];
            }

        // Return the value.
        return value;
    }

    #endregion
}

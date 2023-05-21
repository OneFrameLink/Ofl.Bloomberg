using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using Dapper;
using FastMember;
using Microsoft.Data.SqlClient;

namespace Ofl.Data.SqlClient.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks<T>
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

    private SqlConnection _connection = default!;

    private SqlBulkCopy _bulkCopy = default!;

    private string _table = default!;

    private AsyncEnumerableDataReader<Input<T>> _reader = default!;

    private readonly Input<T>[] _input = new Input<T>[1_000_000];

    private readonly ISqlBulkCopyRowMapper<Input<T>> _mapper = new SqlBulkCopyMapperColumnMapping[] {
        SqlBulkCopyMapperColumnMapping.FromDuckTypedObjectWithMapMethod<T>(
            0
            , new InputDuckTypeMapper<T>()
        )
    }
    .CreateSqlBulkCopyMapper<Input<T>>();


    // FastMember
    private ObjectReader _fastMember = default!;

    #endregion

    #region Parameters

    [Params(
        //1
        //, 10
        //, 100
        //, 1_000
        10_000
        //, 100_000
        //, 1_000_000
    )]
    public int Rows { get; set; }

    [Params(
        1_000
        //100_000
    )]
    public int BatchSize { get; set; }

    [Params(
        true
        //, false
    )]
    public bool EnableStreaming { get; set; }


    #endregion

    #region Setup/teardown

    [GlobalSetup]
    public async Task SetupGlobalAsync()
    {
        // Open the connection and open.
        _connection = new SqlConnection(
            ***REMOVED***
        );
        await _connection
            .OpenAsync()
            .ConfigureAwait(false);

        // Create the table.
        _table = $"TEMP_{Guid.NewGuid():N}";

        // Create the table.
        await _connection
            .ExecuteAsync($@"
create table {_table} (
    Value varchar(100) not null
)
")
            .ConfigureAwait(false);

        // The input.
        //var input = new Input<string> { Value = new string('x', 10) };
        var input = new Input<T> { Value = (T) (object) 100 };

        // Set input.
        for (int i = 0; i < _input.Length; i++)
            _input[i] = input;
    }

    [IterationSetup]
    public void SetupIteration()
    {
        // Create the bulk copy.
        _bulkCopy = new SqlBulkCopy(_connection) {
            BatchSize = BatchSize
            , DestinationTableName = _table
            , EnableStreaming = false
        };

        // Set up the column mapping.
        _bulkCopy.ColumnMappings.Add(0, nameof(Input<T>.Value));

        // Get the segment.
        var segment = new ArraySegment<Input<T>>(_input, 0, Rows);

        // Get the input.
        var asyncEnumerable = segment
            .ToAsyncEnumerable();

        // Create the data reader.
        _reader = new AsyncEnumerableDataReader<Input<T>>(
            asyncEnumerable
            , _mapper
        );

        // Fast member.
        _fastMember = ObjectReader.Create(segment, nameof(Input<string>.Value));
    }

    // BDN does not seem to support async here.
    [IterationCleanup]
    public void CleanupIteration()
    {
        // The implementation.

        // Truncate the table.
        _connection
            .Execute($@"truncate table {_table}");

        // Dispose stuff.
        using var _ = _bulkCopy;
        using var __ = _reader;
        using var ____ = _fastMember;
    }

    [GlobalCleanup]
    public async Task CleanupGlobalAsync()
    {
        // Drop the table.
        await _connection
            .ExecuteAsync($"drop table if exists {_table}")
            .ConfigureAwait(false);

        // Dispose.
        using var _ = _connection;
        using var __ = _bulkCopy;
        using var ___ = _reader;
        using var ____ = _fastMember;
    }

    #endregion

    #region Benchmarks

    [Benchmark(Baseline = true)]
    public async Task<bool> Ofl()
    {
        // Write.
        await _bulkCopy.WriteToServerAsync(_reader).ConfigureAwait(false);

        // Return true.
        return true;
    }

    [Benchmark]
    public async Task<bool> FastMember()
    {
        // Write.
        await _bulkCopy.WriteToServerAsync(_fastMember).ConfigureAwait(false);

        // Return true.
        return true;
    }

    #endregion
}

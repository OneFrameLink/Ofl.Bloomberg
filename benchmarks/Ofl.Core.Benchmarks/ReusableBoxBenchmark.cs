using BenchmarkDotNet.Attributes;

namespace Ofl.Core.Benchmarks;

[MemoryDiagnoser]
public class ReusableBoxBenchmark
{
    #region State

    private ReusableBoxSetup<bool> _BooleanReusableBoxSetup = default!;

    private ReusableBoxSetup<int> _Int32ReusableBoxSetup = default!;

    private ReusableBoxSetup<DateTimeOffset> _DateTimeOffsetReusableBoxSetup = default!;

    private ReusableBoxSetup<long> _Int64ReusableBoxSetup = default!;

    private ReusableBoxSetup<decimal> _DecimalReusableBoxSetup = default!;

    #endregion

    #region Parameters

    [Params(
        1
        , 10
        , 100
        , 1_000
        , 10_000
        , 100_000
        , 1_000_000
        , 10_000_000
    )]
    public int Iterations { get; set; } = default!;

    [Params(
        1
        , 10
        , 100
        , 1_000
    )]
    public int Columns { get; set; } = default!;

    #endregion

    #region Setup/teardown

    [GlobalSetup]
    public void SetupGlobal()
    {
        // Create the setup.
        _Int32ReusableBoxSetup = new ReusableBoxSetup<int>(Columns);
        _DateTimeOffsetReusableBoxSetup = new ReusableBoxSetup<DateTimeOffset>(Columns, DateTimeOffset.Now);
        _DecimalReusableBoxSetup = new ReusableBoxSetup<decimal>(Columns);
        _Int64ReusableBoxSetup = new ReusableBoxSetup<long>(Columns);
        _BooleanReusableBoxSetup = new ReusableBoxSetup<bool>(Columns);
    }

    [GlobalCleanup]
    public void CleanupGlobal()
    {
        // Dispose.
        using var _ = _Int32ReusableBoxSetup;
        using var __ = _DateTimeOffsetReusableBoxSetup;
        using var ___ = _DecimalReusableBoxSetup;
        using var ____ = _Int64ReusableBoxSetup;
        using var _____ = _BooleanReusableBoxSetup;
    }

    #endregion

    #region Benchmarks

    [Benchmark]
    public void Int32_ObjectArray()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _Int32ReusableBoxSetup.Row[c] = 
                _Int32ReusableBoxSetup.TestValue;
    }

    [Benchmark]
    public void Int32_ReusableBox()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
                // Set the value.
                _Int32ReusableBoxSetup.Row[c] = _Int32ReusableBoxSetup
                    .ReusableBoxes[c]
                    .GetBox(ref _Int32ReusableBoxSetup.TestValue);
    }

    [Benchmark]
    public void DateTimeOffset_ObjectArray()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _DateTimeOffsetReusableBoxSetup.Row[c] =
                _DateTimeOffsetReusableBoxSetup.TestValue;
    }

    [Benchmark]
    public void DateTimeOffset_ReusableBox()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _DateTimeOffsetReusableBoxSetup.Row[c] = _DateTimeOffsetReusableBoxSetup
                .ReusableBoxes[c]
                .GetBox(ref _DateTimeOffsetReusableBoxSetup.TestValue);
    }

    [Benchmark]
    public void Decimal_ObjectArray()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _DecimalReusableBoxSetup.Row[c] =
                _DecimalReusableBoxSetup.TestValue;
    }

    [Benchmark]
    public void Decimal_ReusableBox()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _DecimalReusableBoxSetup.Row[c] = _DecimalReusableBoxSetup
                .ReusableBoxes[c]
                .GetBox(ref _DecimalReusableBoxSetup.TestValue);
    }

    [Benchmark]
    public void Int64_ObjectArray()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _Int64ReusableBoxSetup.Row[c] =
                _Int64ReusableBoxSetup.TestValue;
    }

    [Benchmark]
    public void Int64_ReusableBox()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _Int64ReusableBoxSetup.Row[c] = _Int64ReusableBoxSetup
                .ReusableBoxes[c]
                .GetBox(ref _Int64ReusableBoxSetup.TestValue);
    }

    [Benchmark]
    public void Boolean_ObjectArray()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _BooleanReusableBoxSetup.Row[c] =
                _BooleanReusableBoxSetup.TestValue;
    }

    [Benchmark]
    public void Boolean_ReusableBox()
    {
        // Cycle through the iterations.
        for (int i = 0; i < Iterations; i++)
        // Cycle through the columns.
        for (int c = 0; c < Columns; c++)
            // Set the value.
            _BooleanReusableBoxSetup.Row[c] = _BooleanReusableBoxSetup
                .ReusableBoxes[c]
                .GetBox(ref _BooleanReusableBoxSetup.TestValue);
    }

    #endregion
}

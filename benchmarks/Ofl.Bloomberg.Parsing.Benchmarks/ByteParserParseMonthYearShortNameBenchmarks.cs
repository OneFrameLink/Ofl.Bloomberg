using BenchmarkDotNet.Attributes;

namespace Ofl.Bloomberg.Parsing.Benchmarks;

[MemoryDiagnoser]
public class ByteParserParseMonthYearShortNameBenchmarks
{
    #region Parameters

    [ParamsSource(nameof(MonthShortNames))]
    public byte[] MonthShortName { get; set; } = default!;

    public static IEnumerable<byte[]> MonthShortNames => new byte[][] {
        "JAN"u8.ToArray()
        , "FEB"u8.ToArray()
        , "MAR"u8.ToArray()
        , "APR"u8.ToArray()
        , "MAY"u8.ToArray()
        , "JUN"u8.ToArray()
        , "JUL"u8.ToArray()
        , "AUG"u8.ToArray()
        , "SEP"u8.ToArray()
        , "OCT"u8.ToArray()
        , "NOV"u8.ToArray()
        , "DEC"u8.ToArray()
        , "GARBAGE"u8.ToArray()
    };

    #endregion

    #region Benchmarks

    [Benchmark]
    public int MonthShortNameParse() => Parsing.MonthShortName.Parse(MonthShortName);

    #endregion
}

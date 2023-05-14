using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace Ofl.Bloomberg.Parsing.Benchmarks;

[MemoryDiagnoser]
public class ByteParserParseIntegerBigIntegerBenchmarks
{
    #region Parameters

    [ParamsSource(nameof(InputSource))]
    public byte[] Input { get; set; } = default!;

    public static IEnumerable<byte[]> InputSource()
    {
        // Cycle up to 35, 33 is the largest size integer in
        // fields.csv as of April 14th, 2023.
        foreach (var i in Enumerable.Range(0, 35))
            yield return Enumerable
                .Repeat((byte) '0', i)
                .Prepend((byte) '1')
                .ToArray();
    }

    #endregion

    #region Benchmarks

    [Benchmark]
    public BigInteger TryParseIntegerBigInteger()
    {
        // Parse the byte string.
        ReadOnlySpan<byte> span = Input;

        // Parse.
        var _ = SpanParser.TryParseIntegerBigInteger(span, out var value);

        // Return.
        return value;
    }

    #endregion
}

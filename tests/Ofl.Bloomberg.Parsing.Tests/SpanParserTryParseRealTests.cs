using System.Numerics;
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseRealTests
{
    #region Tests

    public static IEnumerable<object[]> Test_Parse_Valid_Input_Parameters()
    {
        // Cycle a few places out either way.
        foreach (var place in Enumerable.Range(1, 9))
        {
            // Get the strings.
            var left = "1" + new string('0', place - 1);
            var right = "." + new string('0', place - 1) + "1";

            // Parse.
            var n = decimal.Parse(left);

            // Create and return.
            yield return new object[] { left, decimal.Parse(left) };
            yield return new object[] { left + ".", decimal.Parse(left) };
            yield return new object[] { left + right, decimal.Parse(left + right) };
            yield return new object[] { right, decimal.Parse(right) };

            // Negative now.
            yield return new object[] { "-" + left, -n };
            yield return new object[] { "-" + left + ".", -n };
            yield return new object[] { "-" + left + right, decimal.Parse("-" + left + right) };
            yield return new object[] { "-" + right, decimal.Parse("-" + right) };
        }

        // Yield min and max.
        yield return new object[] { decimal.MinValue.ToString(), decimal.MinValue };
        yield return new object[] { decimal.MaxValue.ToString(), decimal.MaxValue };
    }

    [Theory]
    [MemberData(nameof(Test_Parse_Valid_Input_Parameters))]
    public void Test_Parse_Valid_Input(
        string input
        , decimal expected
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(input);

        // Parse.
        var result = span.TryParseReal(out var actual, out var parseResultDetail);

        // Assert.
        Assert.True(result);
        Assert.Equal(expected, actual);
        Assert.Equal(ParsingDetail.Ok, parseResultDetail);
    }

    public static TheoryData<string, ParsingDetail> Test_Parse_Invalid_Input_Parameters() =>
        TheoryDataExtensions
            .GetStandardInvalidInputParameters()
            // Not valid all characters.
            .AddInvalidInput("ABC")
            // Not valid, character input in middle of string.
            .AddInvalidInput("1A3")
            // Just period
            .AddInvalidInput(".")
            // Too big.
            .AddInvalidInput((new BigInteger(decimal.MaxValue) * 10).ToString())
            ;

    [Theory]
    [MemberData(nameof(Test_Parse_Invalid_Input_Parameters))]
    public void Test_Parse_Invalid_Input(
        string actualString
        , ParsingDetail expectedParseResultDetail
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(actualString);

        // Parse.
        var result = span.TryParseReal(out _, out var actualParseResultDetail);

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    #endregion
}
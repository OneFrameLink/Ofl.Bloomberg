using System.Numerics;
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseIntegerBigIntegerTests
{
    #region Tests

    public static IEnumerable<object[]> Test_Parse_Valid_Input_Parameters()
    {
        // Cycle to 35 places, create a number and parse.
        foreach (var place in Enumerable.Range(1, 36))
        {
            // Get the string.
            var s = "1" + new string('0', place - 1);

            // Parse.
            var n = BigInteger.Parse(s);

            // Create and return.
            yield return new object[] { s, n };

            // Negative now.
            yield return new object[] { "-" + s, -n };
        }
    }

    [Theory]
    [MemberData(nameof(Test_Parse_Valid_Input_Parameters))]
    public void Test_Parse_Valid_Input(
        string input
        , BigInteger expected
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(input);

        // Parse.
        var result = span.TryParseIntegerBigInteger(out var actual, out var parseResultDetail);

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
        var result = span.TryParseIntegerBigInteger(out _, out var actualParseResultDetail);

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    #endregion
}
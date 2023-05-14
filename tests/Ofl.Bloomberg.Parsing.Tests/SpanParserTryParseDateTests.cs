using Ofl.Bloomberg.Parsing;
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseDateTests
{
    [Theory]
    [InlineData(2019, 2, 1, "20190201")]
    [InlineData(2019, 2, 1, "02/01/2019")]
    public void Test_Parse_Valid_Input(
        int expectedYear
        , int expectedMonth
        , int expectedDay
        , string actualString
    )
    {
        // Get the date.
        var expected = new DateOnly(expectedYear, expectedMonth, expectedDay);

        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(actualString);

        // Parse.
        var result = span.TryParseDate(out var actual, out var parseResultDetail);

        // Assert.
        Assert.True(result);
        Assert.Equal(expected, actual);
        Assert.Equal(ParsingDetail.Ok, parseResultDetail);
    }

    public static TheoryData<string, ParsingDetail> Test_Parse_Invalid_Input_Parameters() =>
        TheoryDataExtensions
            .GetStandardInvalidInputParameters()
            // Integer format, not long enough.
            .AddInvalidInput("1234567")
            // Integer format, too long.
            .AddInvalidInput("123456789")

            // Gobledy gook for a date, but length for integer format
            .AddInvalidInput("A0123456")

            // mm/dd/yyyy format no padding zeros.
            .AddInvalidInput("1/1/1900")

            // mm/dd/yyyy format one less slash and too short.
            .AddInvalidInput("1/1")

            // Gobledy gook for a date, but length for mm/dd/yyyy format
            .AddInvalidInput("A012345678")
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
        var result = span.TryParseDate(out DateOnly _, out var actualParseResultDetail);

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    [Theory]
    [InlineData("99/99/9999")]
    [InlineData("99999999")]
    public void Test_Parse_With_Valid_Format_But_Invalid_Values_Produces_Exception(
        string actualString
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(actualString);

        // Need to wrap in try/catch as spans cannot be accessed in lambdas.
        try
        {
            var _ = span.TryParseDate(out DateOnly _, out var actualParseResultDetail);
        }
        catch
        {
            // Thrown, bail.
            return;
        }

        // Fail.
        Assert.Fail(
            $"An exception was not thrown when calling {nameof(SpanParser.TryParseDate)} with a "
            + $"value of \"{actualString}\"."
        );
    }
}
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseTimeTests
{
    [Theory]
    [InlineData(1, 1, 1, "01:01:01")]
    [InlineData(0, 0, 0, "00:00:00")]
    [InlineData(23, 59, 59, "23:59:59")]
    public void Test_Parse_Valid_Input(
        int expectedHour
        , int expectedMinute
        , int expectedSecond
        , string actualString
    )
    {
        // Get the date.
        var expected = new TimeOnly(expectedHour, expectedMinute, expectedSecond);

        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(actualString);

        // Parse.
        var result = span.TryParseTime(out TimeOnly actual, out var parseResultDetail);

        // Assert.
        Assert.True(result);
        Assert.Equal(expected, actual);
        Assert.Equal(ParsingDetail.Ok, parseResultDetail);
    }

    public static TheoryData<string, ParsingDetail> Test_Parse_Invalid_Input_Parameters() =>
        TheoryDataExtensions
            .GetStandardInvalidInputParameters()
            // Not valid.
            .AddInvalidInput("1234567")

            // Too long.
            .AddInvalidInput("01:01:000")

            // Too litte
            .AddInvalidInput("01:01")

            // Not padded.
            .AddInvalidInput("0:1:1")
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
        var result = span.TryParseTime(out _, out var actualParseResultDetail);

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    [Theory]
    [InlineData("99:99:99")]
    [InlineData("24:00:00")]
    public void Test_Parse_With_Valid_Format_But_Invalid_Values_Produces_Exception(
        string actualString
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(actualString);

        // Need to wrap in try/catch as spans cannot be accessed in lambdas.
        try
        {
            var _ = span.TryParseTime(out TimeOnly _, out var actualParseResultDetail);
        }
        catch
        {
            // Thrown, bail.
            return;
        }

        // Fail.
        Assert.Fail(
            $"An exception was not thrown when calling {nameof(SpanParser.TryParseTime)} with a "
            + $"value of \"{actualString}\"."
        );
    }
}
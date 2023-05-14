using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseIntegerRealInt32Tests
{
    #region Tests

    public static IEnumerable<object[]> Test_Parse_Valid_Input_Parameters() =>
        SpanParserTryParseIntegerRealExtensions
            .GetByteParserTryParseIntegerRealTestParameters<int>(int.MaxValue);

    [Theory]
    [MemberData(nameof(Test_Parse_Valid_Input_Parameters))]
    public void Test_Parse_Valid_And_Oversized_Input(
        string input
        , int expectedInteger
        , bool expectedIntegerParsed
        , decimal expectedReal
        , bool expectedRealParsed
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(input);

        // Parse.
        var actualResult = span.TryParseIntegerRealInt32(
            out int actualInteger
            , out decimal actualReal
            , out ParsingDetail parseResultDetail
        );

        // Figure out what the actual result should be.
        var expectedResult = 
            (expectedIntegerParsed ? ParseIntegerRealResult.Integer : ParseIntegerRealResult.None)
            | (expectedRealParsed ? ParseIntegerRealResult.Real : ParseIntegerRealResult.None);

        // Assert.
        Assert.Equal(expectedResult, actualResult);

        // Compare integer and real parts.
        if (expectedIntegerParsed)
            Assert.Equal(expectedInteger, actualInteger);
        if (expectedRealParsed)
            Assert.Equal(expectedReal, actualReal);

        // The parsing detail is fine.
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
        var result = span.TryParseIntegerRealInt32(
            out _
            , out _
            , out var actualParseResultDetail
        );

        // Assert.
        Assert.Equal(ParseIntegerRealResult.None, result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    #endregion
}
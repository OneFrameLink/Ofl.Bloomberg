using Ofl.Bloomberg.Parsing;
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseBooleanTests
{
    #region Tests

    [Theory]
    [InlineData('Y', true)]
    [InlineData('N', false)]
    public void Test_Parse_Valid_Input(
        char input
        , bool expected
    )
    {
        // Get the input span first.
        ReadOnlySpan<byte> inputSpan = Encoding
            .ASCII
            .GetBytes(new char[] { input });

        // Parse.
        var result = inputSpan.TryParseBoolean(
            out var actual
            , out var actualDetail
        );

        // Assert.
        Assert.True(result);
        Assert.Equal(expected, actual);
        Assert.Equal(ParsingDetail.Ok, actualDetail);
    }

    public static TheoryData<string, ParsingDetail> Test_Parse_Invalid_Input_Parameters() =>
        TheoryDataExtensions
            .GetStandardInvalidInputParameters()
            // Too long false
            .AddChained("NO", ParsingDetail.Ok)

            // Number true/false
            .AddChained("1", ParsingDetail.Ok)
            .AddChained("0", ParsingDetail.Ok)

            // True/false
            .AddChained("TRUE", ParsingDetail.Ok)
            .AddChained("FALSE", ParsingDetail.Ok)

            // Too long true
            .AddChained("YES", ParsingDetail.Ok)
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
        var result = span.TryParseBoolean(
            out _
            , out var actualParseResultDetail
        );

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    #endregion
}
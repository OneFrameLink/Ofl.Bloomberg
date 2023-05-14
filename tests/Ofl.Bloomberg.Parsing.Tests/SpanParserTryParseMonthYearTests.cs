using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseMonthYearTests
{
    #region Tests

    public static IEnumerable<object[]> Test_Parse_Valid_Input_Parameters()
    {
        // The year.
        var year = 23;

        // The short name months.
        yield return new object[] { $"JAN {year}", 1, year, "" };
        yield return new object[] { $"FEB {year}", 2, year, "" };
        yield return new object[] { $"MAR {year}", 3, year, "" };
        yield return new object[] { $"APR {year}", 4, year, "" };
        yield return new object[] { $"MAY {year}", 5, year, "" };
        yield return new object[] { $"JUN {year}", 6, year, "" };
        yield return new object[] { $"JUL {year}", 7, year, "" };
        yield return new object[] { $"AUG {year}", 8, year, "" };
        yield return new object[] { $"SEP {year}", 9, year, "" };
        yield return new object[] { $"OCT {year}", 10, year, "" };
        yield return new object[] { $"NOV {year}", 11, year, "" };
        yield return new object[] { $"DEC {year}", 12, year, "" };

        // Now cycle through month numbers.
        foreach (var month in Enumerable.Range(1, 12))
        {
            // Yield two digit month and year
            yield return new object[] { $"{month:D2}/{year}", month, year, "" };

            // Now with a period.
            yield return new object[] { $"{month:D2}/{year} Q1", month, year, "Q1" };
        }
    }

    [Theory]
    [MemberData(nameof(Test_Parse_Valid_Input_Parameters))]
    public void Test_Parse_Valid_Input(
        string input
        , int expectedMonth
        , int expectedYear
        , string expectedPeriod
    )
    {
        // Get the input span first.
        ReadOnlySpan<byte> inputSpan = Encoding
            .ASCII
            .GetBytes(input);

        // Parse.
        var result = inputSpan.TryParseMonthYear(
            out var actualMonth
            , out var actualYear
            , out var actualPeriodSpan
            , out var actualDetail
        );

        // Assert.
        Assert.True(result);
        Assert.Equal(expectedMonth, actualMonth);
        Assert.Equal(expectedYear, actualYear);
        Assert.Equal(Encoding.ASCII.GetString(actualPeriodSpan.ToArray()), expectedPeriod);
        Assert.Equal(ParsingDetail.Ok, actualDetail);
    }

    public static TheoryData<string, ParsingDetail> Test_Parse_Invalid_Input_Parameters() =>
        TheoryDataExtensions
            .GetStandardInvalidInputParameters()
            // Not valid all characters.
            .AddInvalidInput("ABC")
            // Not valid, character input in middle of string.
            .AddInvalidInput("1A3")
            // No padding for month...
            .AddInvalidInput("1/15")

            // Or year...
            .AddInvalidInput("01/1")

            // Begins with valid month/year but no space
            .AddInvalidInput("12/15/")

            // Short name, with period.
            .AddInvalidInput("DEC 15 Q1")

            // Lower case short name.
            .AddInvalidInput("Jan 15")
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
        var result = span.TryParseMonthYear(
            out _
            , out _
            , out _
            , out var actualParseResultDetail
        );

        // Assert.
        Assert.False(result);
        Assert.Equal(expectedParseResultDetail, actualParseResultDetail);
    }

    #endregion
}
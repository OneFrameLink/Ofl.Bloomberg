using System.Numerics;
using System.Text;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseIntegerRealBigIntegerTests
{
    #region Tests

    public static IEnumerable<object[]> Test_Parse_Valid_Input_Parameters()
    {
        // Creates the parameters.
        static object[] CreateParameters(
            string input
            , BigInteger expectedInteger
            , bool expectedIntegerParsed
            , decimal expectedReal
            , bool expectedRealParsed
        ) => new object[] {
            input
            , expectedInteger
            , expectedIntegerParsed
            , expectedReal
            , expectedRealParsed
        };

        // Cycle to 35 places, create a number and parse.
        foreach (var place in Enumerable.Range(1, 36))
        {
            // Get the string.
            var s = "1" + new string('0', place - 1);

            // Parse.
            var n = BigInteger.Parse(s);

            // Can this fit in a decimal?
            bool fitInDecimal = n <= new BigInteger(decimal.MaxValue);

            // Create and return.
            yield return CreateParameters(
                s
                , n
                , true
                , fitInDecimal ? (decimal) n : 0
                , fitInDecimal
            );

            // Update s, n.
            s = "-" + s;
            n = -n;

            // Can this fit in a decimal?
            fitInDecimal = n >= new BigInteger(decimal.MinValue);

            // Negative now.
            yield return CreateParameters(
                s
                , n
                , true
                , fitInDecimal ? (decimal) n : 0
                , fitInDecimal
            );
        }

        // Now for the real part.
        foreach (var place in Enumerable.Range(1, 9))
        {
            // Get the strings.
            var left = "1" + new string('0', place - 1);
            var right = "." + new string('0', place - 1) + "1";

            // Parse.
            var n = decimal.Parse(left);

            // Create and return.
            yield return CreateParameters(
                left
                , new BigInteger(n)
                , true
                , n
                , true
            );
            yield return CreateParameters(
                left + "."
                , default
                , false
                , n
                , true
            );
            yield return CreateParameters(
                left + right
                , default
                , false
                , decimal.Parse(left + right) 
                , true
            );
            yield return CreateParameters(
                right
                , default
                , false
                , decimal.Parse(right)
                , true
            );

            // Negative now.
            yield return CreateParameters(
                "-" + left
                , new BigInteger(-n)
                , true
                , -n
                , true
            );
            yield return CreateParameters(
                "-" + left + "."
                , default
                , false
                , -n
                , true
            );
            yield return CreateParameters(
                "-" + left + right
                , default
                , false
                , decimal.Parse("-" + left + right) 
                , true
            );
            yield return CreateParameters(
                "-" + right
                , default
                , false
                , decimal.Parse("-" + right)
                , true
            );
        }

        // Yield min and max.
        yield return CreateParameters(
            decimal.MinValue.ToString()
            , new BigInteger(decimal.MinValue)
            , true
            , decimal.MinValue
            , true
        );
        yield return CreateParameters(
            decimal.MaxValue.ToString()
            , new BigInteger(decimal.MaxValue)
            , true
            , decimal.MaxValue
            , true
        );
    }

    [Theory]
    [MemberData(nameof(Test_Parse_Valid_Input_Parameters))]
    public void Test_Parse_Valid_Input(
        string input
        , BigInteger expectedInteger
        , bool expectedIntegerParsed
        , decimal expectedReal
        , bool expectedRealParsed
    )
    {
        // Get the span in ascii bytes.
        ReadOnlySpan<byte> span = Encoding.ASCII.GetBytes(input);

        // Parse.
        var actualResult = span.TryParseIntegerRealBigInteger(
            out BigInteger actualInteger
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
        var result = span.TryParseIntegerRealBigInteger(
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
using System.Numerics;

namespace Ofl.Bloomberg.Parsing.Tests;

public class SpanParserTryParseIntegerRealExtensions
{
    #region Tests

    public static IEnumerable<object[]> GetByteParserTryParseIntegerRealTestParameters<T>(
        BigInteger maxValue
    )
        where T : struct
            , IMinMaxValue<T>
            , IParsable<T>
            , IDivisionOperators<T, T, T>
            , IConvertible

    {
        // Creates the parameters.
        static object[] CreateParameters(
            string input
        )
        {
            // Parse each.
            var integerParsed = T.TryParse(input, null, out var integer);
            var realParsed = decimal.TryParse(input, out var real);

            // Return the parameters.
            return new object[] {
                input
                , integer
                , integerParsed
                , real
                , realParsed
            };

        }

        // Get the max value and min value.  Get the max power.
        // Add one to account for the fact that no value of x digits
        // can equal the value of 10 to x + 1 digits.
        var integerMaxPower = ((int) BigInteger.Log10(maxValue)) + 1;
        var decimalMaxPower = ((int) BigInteger.Log10(new BigInteger(decimal.MaxValue))) + 1;

        // Add five so we are *sure* we're expanding beyond both.
        var power = int.Max(integerMaxPower, decimalMaxPower) + 5;

        // Cycle to places beyond the max power
        foreach (var place in Enumerable.Range(1, power))
        {
            // Get the string.
            var s = "1" + new string('0', place - 1);

            // Create and return.
            yield return CreateParameters(s);

            // Negative now.
            s = "-" + s;
            yield return CreateParameters(s);
        }

        // Now for the real part.
        foreach (var place in Enumerable.Range(1, 50))
        {
            // Get the strings.
            var left = "1" + new string('0', place - 1);
            var right = "." + new string('0', place - 1) + "1";

            // Integer with decimal point.
            yield return CreateParameters(left + ".");

            // Integer and decimal places.
            yield return CreateParameters(left + right);

            // Decimal point with leading zero.
            yield return CreateParameters("0" + right);

            // Decimal without leading zero.
            yield return CreateParameters(right);

            // Now negative for each ??
            yield return CreateParameters("-" + left + ".");
            yield return CreateParameters("-" + left + right);
            yield return CreateParameters("-0" + right);
            yield return CreateParameters("-" + right);
        }

        // Yield min and max.
        yield return CreateParameters(decimal.MinValue.ToString());
        yield return CreateParameters(decimal.MaxValue.ToString());
        yield return CreateParameters(T.MinValue.ToString()!);
        yield return CreateParameters(T.MaxValue.ToString()!);
    }

    #endregion
}
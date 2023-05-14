using System.ComponentModel.DataAnnotations;

namespace Ofl.Core.Tests;

// NOTES
// DateTime - long - ToBinary, FromBinary
// DateOnly - int - DayNumber, constructor taking daynumber
// TimeOnly - long - Ticks, constructor taking ticks
// DateTimeOffset - Combination of DateTime and Offset (TimeSpan) or _offsetMinutes (short), maybe create structure that holds this?
// Can we possibly have a class with an impicit cast?  Maybe an override of GetType?
public class ReusableBoxTests
{
    #region Helpers

    private static void AssertReusableBox<T>(
        T expected
    ) where T : struct
    {
        // Create a box
        var box = new ReusableBox<T>();

        // Get the boxed value.
        var actual = box.GetBox(ref expected);

        // Assert.
        Assert.Equal(expected, actual);
    }

    #endregion

    #region Tests

    [Fact]
    public void Test_Int32() => AssertReusableBox(Random.Shared.Next());

    [Fact]
    public void Test_Int64() => AssertReusableBox(Random.Shared.NextInt64());

    [Fact]
    public void Test_DateTime() => AssertReusableBox(DateTime.Now);

    [Fact]
    public void Test_Decimal() => AssertReusableBox((decimal) Random.Shared.NextDouble());

    [Fact]
    public void Test_Double() => AssertReusableBox(Random.Shared.NextDouble());

    [Fact]
    public void Test_DateTimeOffset() => AssertReusableBox(DateTimeOffset.Now);

    #endregion
}
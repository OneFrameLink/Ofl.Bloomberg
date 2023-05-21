namespace Ofl.Core.Tests;

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
    public void Test_Int32_Max() => AssertReusableBox(int.MaxValue);

    [Fact]
    public void Test_Int32_Min() => AssertReusableBox(int.MinValue);


    [Fact]
    public void Test_Int64() => AssertReusableBox(Random.Shared.NextInt64());

    [Fact]
    public void Test_Int64_Max() => AssertReusableBox(long.MaxValue);

    [Fact]
    public void Test_Int64_Min() => AssertReusableBox(long.MinValue);


    [Fact]
    public void Test_DateTime() => AssertReusableBox(DateTime.Now);

    [Fact]
    public void Test_DateTime_Max() => AssertReusableBox(DateTime.MaxValue);

    [Fact]
    public void Test_DateTime_Min() => AssertReusableBox(DateTime.MinValue);


    [Fact]
    public void Test_Decimal() => AssertReusableBox((decimal) Random.Shared.NextDouble());

    [Fact]
    public void Test_Decimal_Max() => AssertReusableBox(decimal.MaxValue);

    [Fact]
    public void Test_Decimal_Min() => AssertReusableBox(decimal.MinValue);


    [Fact]
    public void Test_Double() => AssertReusableBox(Random.Shared.NextDouble());

    [Fact]
    public void Test_Double_Max() => AssertReusableBox(double.MaxValue);

    [Fact]
    public void Test_Double_Min() => AssertReusableBox(double.MaxValue);


    [Fact]
    public void Test_DateTimeOffset() => AssertReusableBox(DateTimeOffset.Now);

    [Fact]
    public void Test_DateTimeOffset_Max() => AssertReusableBox(DateTimeOffset.MaxValue);

    [Fact]
    public void Test_DateTimeOffset_Min() => AssertReusableBox(DateTimeOffset.MinValue);

    #endregion
}
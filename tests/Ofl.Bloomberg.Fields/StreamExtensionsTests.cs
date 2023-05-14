using Ofl.Bloomberg.Fields.Tests.Files;

namespace Ofl.Bloomberg.Fields;

public class StreamExtensionsTests
{
    [Fact]
    public async Task Test_EnumerateFields_Async()
    {
        // Get the stream.
        using var stream = Resource.GetFieldsCsvResourceStream();

        // Get the enumerable.
        var fields = await stream
            .EnumerateFields()
            .ToListAsync()
            .ConfigureAwait(false);

        // Assert.
        Assert.Equal(42944, fields.Count);
    }

    [Theory]
    [InlineData("ID135", "ID_BB_GLOBAL", true, true, true, true, true, true, true, true, true, true, 12, 0, "Character")]
    public async Task Test_EnumerateFields_Map_Async(
        string id
        , string mnemonic
        , bool? comdty
        , bool? equity
        , bool? muni
        , bool? pfd
        , bool? mMkt
        , bool? govt
        , bool? corp
        , bool? index
        , bool? curncy
        , bool? mtge
        , int standardWidth
        , int standardDecimalPlaces
        , string fieldType
    )
    {
        // Get the stream.
        using var stream = Resource.GetFieldsCsvResourceStream();

        // Get the enumerable.
        var fields = await stream
            .EnumerateFields()
            .ToListAsync()
            .ConfigureAwait(false);

        // Get the field.
        var field = fields.Single(f => f.FieldId == id);

        // Asset.
        Assert.Equal(mnemonic, field.FieldMnemonic);
        Assert.Equal(comdty, field.Comdty);
        Assert.Equal(equity, field.Equity);
        Assert.Equal(muni, field.Muni);
        Assert.Equal(pfd, field.Pfd);
        Assert.Equal(mMkt, field.MMkt);
        Assert.Equal(govt, field.Govt);
        Assert.Equal(corp, field.Corp);
        Assert.Equal(index, field.Index);
        Assert.Equal(curncy, field.Curncy);
        Assert.Equal(mtge, field.Mtge);
        Assert.Equal(standardWidth, field.StandardWidth);
        Assert.Equal(standardDecimalPlaces, field.StandardDecimalPlaces);
        Assert.Equal(fieldType, field.FieldType);
    }
}
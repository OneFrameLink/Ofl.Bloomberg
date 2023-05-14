using Ofl.Bloomberg.BackOffice.Tests.Files;

namespace Ofl.Bloomberg.BackOffice.Tests;

public class BloombergBackOfficeReaderMsg1Parsing1600MuniOutFileStreamTests
{
    #region Tests

    [Fact]
    public async Task Test_ReadStartOfFile_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file with headers.
        var headers = await reader.ReadStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Assert the headers.
        Assert.Equal(3, headers.Count);
        Assert.Equal("getdata", headers["PROGRAMNAME"]);
        Assert.Equal("16:00", headers["SNAPTIME"]);
        Assert.Equal("UTF-8", headers["ENCODING"]);
    }

    [Fact]
    public async Task Test_IgnoreStartOfFileAsync_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );
    }

    [Fact]
    public async Task Test_ReadFields_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Read the fields.
        var fields = await reader
            .ReadFieldsAsync(default)
            .ConfigureAwait(false);

        // There are fields.
        Assert.NotEmpty(fields);
    }

    [Fact]
    public async Task Test_ReadKeyValuePairsUntil_Up_To_StartOfData_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Read the fields.
        await reader
            .IgnoreFieldsAsync(default)
            .ConfigureAwait(false);

        // Get the headers.
        var headers = await reader.ReadKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.StartOfData
            , default
        )
        .ConfigureAwait(false);

        // Assert.
        Assert.Single(headers);
        Assert.Equal("Thu Mar 16 16:08:11 EDT 2023", headers["TIMESTARTED"]);
    }

    [Fact]
    public async Task Test_EnumerateDataSection_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Read the fields.
        await reader
            .IgnoreFieldsAsync(default)
            .ConfigureAwait(false);

        // Get the headers.
        await reader.IgnoreKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.StartOfData
            , default
        )
        .ConfigureAwait(false);

        // The count.
        var count = 0;

        // Cycle through everything.
        await foreach (var row in reader.EnumerateDataSection<BloombergBackOfficeDescriptiveDataFileRow>())
            count++;

        // The rows.
        Assert.Equal(1583, count);
    }

    [Fact]
    public async Task Test_ReadKeyValuePairsUntil_After_Data_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Read the fields.
        await reader
            .IgnoreFieldsAsync(default)
            .ConfigureAwait(false);

        // Get the headers.
        await reader.IgnoreKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.StartOfData
            , default
        )
        .ConfigureAwait(false);

        // Cycle through everything.
        await foreach (var row in reader.EnumerateDataSection<BloombergBackOfficeDescriptiveDataFileRow>())
        { }

        // Get the headers.
        var headers = await reader.ReadKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.EndOfFile
            , default
        )
        .ConfigureAwait(false);

        // Assert the headers.
        Assert.Equal(2, headers.Count);
        Assert.Equal("1583", headers["DATARECORDS"]);
        Assert.Equal("Thu Mar 16 16:25:29 EDT 2023", headers["TIMEFINISHED"]);
    }

    [Fact]
    public async Task Test_ReadEndOfFile_Async()
    {
        // Get the stream.
        using var stream = Resource.GetMsg1Parsing1600MuniOutFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file without headers.
        await reader.IgnoreStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Read the fields.
        await reader
            .IgnoreFieldsAsync(default)
            .ConfigureAwait(false);

        // Get the headers.
        await reader.IgnoreKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.StartOfData
            , default
        )
        .ConfigureAwait(false);

        // Cycle through everything.
        await foreach (var row in reader.EnumerateDataSection<BloombergBackOfficeDescriptiveDataFileRow>())
        { }

        // Ignore the key value pairs.
        await reader.IgnoreKeyValuePairsUntilAsync(
            BloombergBackOfficeSection.EndOfFile
            , default
        )
        .ConfigureAwait(false);

        // Read end of file.
        await reader
            .ReadEndOfFileAsync(default)
            .ConfigureAwait(false);
    }

    #endregion
}

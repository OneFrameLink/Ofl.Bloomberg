using Ofl.Bloomberg.BackOffice.Tests.Files;

namespace Ofl.Bloomberg.BackOffice.Tests;

public class BloombergBackOfficeReaderCreditRiskDifFileStreamTests
{
    #region Tests

    [Fact]
    public async Task Test_ReadStartOfFile_Async()
    {
        // Get the stream.
        using var stream = Resource.GetCreditRiskDifFileStream();

        // Create the reader.
        using var reader = new BloombergBackOfficeReader(stream);

        // Read start of file with headers.
        var headers = await reader.ReadStartOfFileAsync(
            BloombergBackOfficeSection.StartOfFields
            , default
        );

        // Assert the headers.
        Assert.Equal(2, headers.Count);
        Assert.Equal("getdata", headers["PROGRAMNAME"]);
        Assert.Equal("yyyymmdd", headers["DATEFORMAT"]);
    }

    [Fact]
    public async Task Test_IgnoreStartOfFileAsync_Async()
    {
        // Get the stream.
        using var stream = Resource.GetCreditRiskDifFileStream();

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
        using var stream = Resource.GetCreditRiskDifFileStream();

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
        using var stream = Resource.GetCreditRiskDifFileStream();

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
        Assert.Equal("Fri Apr  7 17:40:19 EDT 2023", headers["TIMESTARTED"]);
    }

    [Fact]
    public async Task Test_EnumerateDataSection_Async()
    {
        // Get the stream.
        using var stream = Resource.GetCreditRiskDifFileStream();

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

        // For reading the ID of a row.
        static int GetKey(in BloombergBackOfficeDescriptiveDataFileRow row) =>
            row.SecurityDescription.Parse<int>();

        // The hash set for ID_BB_COMPANY.
        var items = new HashSet<int>();

        // Cycle through everything.
        await foreach (var row in reader.EnumerateDataSection<BloombergBackOfficeDescriptiveDataFileRow>())
        {
            // Read the key
            var key = GetKey(row);

            // Add.
            Assert.True(
                items.Add(key)
                , $"The ID_BB_COMPANY of {key} has already been added."
            );
        }

        // The rows.
        Assert.Equal(11245, items.Count);
    }

    [Fact]
    public async Task Test_ReadKeyValuePairsUntil_After_Data_Async()
    {
        // Get the stream.
        using var stream = Resource.GetCreditRiskDifFileStream();

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
        Assert.Equal("11245", headers["DATARECORDS"]);
        Assert.Equal("Fri Apr  7 17:40:58 EDT 2023", headers["TIMEFINISHED"]);
    }

    [Fact]
    public async Task Test_ReadEndOfFile_Async()
    {
        // Get the stream.
        using var stream = Resource.GetCreditRiskDifFileStream();

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

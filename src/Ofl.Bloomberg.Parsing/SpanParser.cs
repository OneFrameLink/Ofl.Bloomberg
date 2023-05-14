using System.Buffers.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ofl.Bloomberg.Parsing;

// NOTE: Driven by
// https://data.bloomberg.com/docs/data-license/#per-security-DataTypes
public static class SpanParser
{
    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParsingDetail ParseDetail(
        this in ReadOnlySpan<byte> source
    )
    {
        // Is it empty or whitespace?  If so, not applicable.
        if (source.IsEmpty || source.IndexOfAnyExcept(NonAlphaNumericByte.Space) < 0)
            return ParsingDetail.FieldSecurityCombinationNotApplicable;

        // Data missing.
        if (source.SequenceEqual(ParsingDetailValue.DataMissing))
            return ParsingDetail.DataMissing;

        // Not downloadable.
        if (source.SequenceEqual(ParsingDetailValue.NotDownloadable))
            return ParsingDetail.NotDownloadable;

        if (source.SequenceEqual(ParsingDetailValue.NotSubscribed))
            return ParsingDetail.NotSubscribed;

        if (source.SequenceEqual(ParsingDetailValue.FieldUnknown))
            return ParsingDetail.FieldUnknown;

        // Ok for now.
        return ParsingDetail.Ok;
    }

    #endregion

    #region Date parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseDateIntegerFormat(
        this in ReadOnlySpan<byte> source
        , out DateOnly value
    )
    {
        // Format is yyyymmdd
        if (
            // Length.
            source.Length == 8
            // Year.
            && Utf8Parser.TryParse(source[..4], out int year, out var bytesConsumed)
            && bytesConsumed == 4
            // Month.
            && Utf8Parser.TryParse(source.Slice(4, 2), out int month, out bytesConsumed)
            && bytesConsumed == 2
            // Day
            && Utf8Parser.TryParse(source.Slice(6, 2), out int day, out bytesConsumed)
            && bytesConsumed == 2
        )
        {
            // Set the date.
            value = new(year, month, day);

            // Return true.
            return true;
        }

        // Default date.
        value = DateOnly.MinValue;

        // Cannot parse, bail.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseDateMonthDayYearFormat(
        this in ReadOnlySpan<byte> source
        , out DateOnly value
    )
    {
        // Format is mm/dd/yyyy
        if (
            // Length.
            source.Length == 10
            // Month.
            && Utf8Parser.TryParse(source[..2], out int month, out var bytesConsumed)
            && bytesConsumed == 2
            // Separator
            && source.Slice(2, 1)[0] == NonAlphaNumericByte.ForwardSlash
            // Day
            && Utf8Parser.TryParse(source.Slice(3, 2), out int day, out bytesConsumed)
            && bytesConsumed == 2
            // Separator
            && source.Slice(5, 1)[0] == NonAlphaNumericByte.ForwardSlash
            // Year
            && Utf8Parser.TryParse(source.Slice(6, 4), out int year, out bytesConsumed)
            && bytesConsumed == 4
        )
        {
            // Set the date.
            value = new(year, month, day);

            // Return true.
            return true;
        }

        // Default date.
        value = DateOnly.MinValue;

        // Cannot parse, bail.
        return false;
    }

    /// <remarks>
    /// <para>This method will not acccount for dates that have
    /// invalid days/months/years and result in exceptions.
    /// </para>
    /// <para>
    /// For example, the following will throw an exception when used as input:
    /// 
    /// <list type="bullet">
    /// <item>
    ///     <description>99999999</description>
    /// </item>
    /// <item>
    ///     <description>99/99/9999</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>If these invalid values are required, then it is recommended that
    /// the <paramref name="source"/> be processed in a custom manner.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySpan<byte> source
        , out DateOnly value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default, we use no matter what.
        detail = ParsingDetail.Ok;

        // Parsing of the formats can be done first because
        // it's not possible for the parse result detail
        // to exist in those formats.
        if (
            source.TryParseDateIntegerFormat(out value)
            || source.TryParseDateMonthDayYearFormat(out value)
        )
            // Assume the parse result detail is ok.
            return true;

        // If parsing the parse result detail.
        if (parseDetail)
            // Get the parse result detail.
            detail = source.ParseDetail();

        // Bail.
        return false;
    }

    /// <inheritdoc cref="TryParseDate(in ReadOnlySpan{byte}, out DateOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySpan<byte> source
        , out DateOnly value
        , out ParsingDetail detail
    ) => source.TryParseDate(out value, true, out detail);

    /// <inheritdoc cref="TryParseDate(in ReadOnlySpan{byte}, out DateOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySpan<byte> source
        , out DateOnly value
    ) => source.TryParseDate(out value, false, out _);

    #endregion

    #region Time parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTimeHourMinuteSecondFormat(
        this in ReadOnlySpan<byte> source
        , out TimeOnly value
    )
    {
        // Format is hh:mm:ss
        if (
            // Length.
            source.Length == 8
            // Hour.
            && Utf8Parser.TryParse(source[..2], out int hour, out var bytesConsumed)
            && bytesConsumed == 2
            // Separator
            && source.Slice(2, 1)[0] == NonAlphaNumericByte.Colon
            // Minute
            && Utf8Parser.TryParse(source.Slice(3, 2), out int minute, out bytesConsumed)
            && bytesConsumed == 2
            // Separator
            && source.Slice(5, 1)[0] == NonAlphaNumericByte.Colon
            // Second
            && Utf8Parser.TryParse(source.Slice(6, 2), out int second, out bytesConsumed)
            && bytesConsumed == 2
        )
        {
            // Set the time.
            value = new(hour, minute, second);

            // Return true.
            return true;
        }

        // Default date.
        value = TimeOnly.MinValue;

        // Cannot parse, bail.
        return false;
    }

    /// <remarks>
    /// <para>This method will not acccount for times that have
    /// invalid hours/minutes/seconds and result in exceptions.
    /// </para>
    /// <para>
    /// For example, the following will throw an exception when used as input:
    /// 
    /// <list type="bullet">
    /// <item>
    ///     <description>99:99:99</description>
    /// </item>
    /// <item>
    ///     <description>24:00:00</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>If these invalid values are required, then it is recommended that
    /// the <paramref name="source"/> be processed in a custom manner.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySpan<byte> source
        , out TimeOnly value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Assume the parse result detail is ok.
        detail = ParsingDetail.Ok;

        // Parsing these formats can be done first because
        // it's not possible for the parse result detail
        // to exist in those formats.
        if (
            source.TryParseTimeHourMinuteSecondFormat(out value)
        )
            return true;

        // Set the parse result detail if parsing.
        if (parseDetail)
            detail = source.ParseDetail();

        // Bail.
        return false;
    }

    /// <inheritdoc cref="TryParseTime(in ReadOnlySpan{byte}, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySpan<byte> source
        , out TimeOnly value
        , out ParsingDetail detail
    ) => source.TryParseTime(out value, true, out detail);

    /// <inheritdoc cref="TryParseTime(in ReadOnlySpan{byte}, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySpan<byte> source
        , out TimeOnly value
    ) => source.TryParseTime(out value, false, out _);

    #endregion

    #region Date or time parsing.

    /// <remarks>
    /// <para>This method has the same exception behavior when calling
    /// <see cref="TryParseDate(in ReadOnlySpan{byte}, out DateOnly, out ParsingDetail)"/>
    /// and <see cref="TryParseTime(in ReadOnlySpan{byte}, out TimeOnly, out ParsingDetail)"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySpan<byte> source
        , out DateOnly date
        , out TimeOnly time
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default everything necessary.
        time = TimeOnly.MaxValue;

        // Parsing these formats can be done first because
        // it's not possible for the parse result detail
        // to exist in those formats.
        // Parse date formats first because they are longer.
        if (source.TryParseDate(out date, out detail))
            return ParseDateOrTimeResult.Date;

        // Time.
        if (source.TryParseTime(out time, out detail))
            return ParseDateOrTimeResult.Time;  

        // Get the parse result detail.
        if (parseDetail)
            detail = source.ParseDetail();

        // Bail.
        return ParseDateOrTimeResult.None;
    }

    /// <inheritdoc cref="TryParseDateOrTime(in ReadOnlySpan{byte}, out DateOnly, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySpan<byte> source
        , out DateOnly date
        , out TimeOnly time
        , out ParsingDetail detail
    ) => source.TryParseDateOrTime(out date, out time, true, out detail);

    /// <inheritdoc cref="TryParseDateOrTime(in ReadOnlySpan{byte}, out DateOnly, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySpan<byte> source
        , out DateOnly date
        , out TimeOnly time
    ) => source.TryParseDateOrTime(out date, out time, false, out _);

    #endregion

    #region Month/year parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYearForwardSlashFormat(
        this in ReadOnlySpan<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
    )
    {
        // Default.
        month = default;
        year = default;
        period = default;

        // If we can't get the month/year then
        // just bail.
        if (!(
            // Length.
            source.Length >= 5
            // Two digit month.
            && Utf8Parser.TryParse(source[..2], out month, out int bytesConsumed)
            && bytesConsumed == 2
            // Separator.
            && source.Slice(2, 1)[0] == NonAlphaNumericByte.ForwardSlash
            // Two digit year.
            && Utf8Parser.TryParse(source.Slice(3, 2), out year, out bytesConsumed)
            && bytesConsumed == 2
        ))
            return false;

        // There's a year and a month.  If the this is just five characters, bail.
        if (source.Length == 5)
            return true;

        // Get the rest.
        var rest = source[5..];

        // Find the first non space character index.
        var firstNonSpaceCharacterIndex = rest.IndexOfAnyExcept(NonAlphaNumericByte.Space);

        // If the first space is not found, or not the first character
        // then bail.
        if (firstNonSpaceCharacterIndex <= 0)
            return false;

        // Set the period to everything from the rest at the first non space index.
        // Set to empty if it's all whitepsace.
        period = firstNonSpaceCharacterIndex < 0
            ? default
            : rest[firstNonSpaceCharacterIndex..];

        // Return true.
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYearShortNameFormat(
        this in ReadOnlySpan<byte> source
        , out int month
        , out int year
    )
    {
        // Default.
        month = default;
        year = default;

        // If the length is not 6, bail.
        if (source.Length != 6)
            return false;

        // Is there a space at index 3?  If not, bail.
        if (source[3] != NonAlphaNumericByte.Space)
            return false;

        // Try and get the year, if not parsable, bail.
        if (!(
            Utf8Parser.TryParse(source[4..], out year, out int bytesConsumed)
            && bytesConsumed == 2
        ))
            return false;

        // Get the month.
        var monthSpan = source[..3];

        // Parse.
        month = monthSpan.Parse();

        // If non 0, return true.
        return month != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySpan<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default everything.
        detail = ParsingDetail.Ok;
        month = default;
        year = default;
        period = default;

        // As with other formatted items, we parse directly first
        // instead of getting parsing detail, since the formats
        // are mutually exclusive.
        // Is there a forward slash at 2?
        var forwardSlashAt2 = source.Length >= 3 && source[2] == NonAlphaNumericByte.ForwardSlash;

        // If the forward slash is at 2, try to parse further.
        if (forwardSlashAt2)
            // True or false, we can return, since a slash indicates that it can't
            // be any of the error codes.
            return source.TryParseMonthYearForwardSlashFormat(out month, out year, out period);

        // Is there a space at 3?
        var spaceAt3 = source.Length >= 4 && source[3] == NonAlphaNumericByte.Space;

        // If there is a space at 3, then try and parse that.
        // Also, direc
        if (spaceAt3)
        {
            // Get the result.
            var result = source.TryParseMonthYearShortNameFormat(out month, out year);

            // If *not* parsing detail or the result is true, we can bail.
            if (!parseDetail || result) return result;
        }

        // Both failed, if parsing detail, do so here.
        if (parseDetail)
            detail = source.ParseDetail();

        // Return.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySpan<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
        , out ParsingDetail detail
    ) => source.TryParseMonthYear(out month, out year, out period, true, out detail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySpan<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
    ) => source.TryParseMonthYear(out month, out year, out period, false, out _);

    #endregion

    #region Boolean parsing


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySpan<byte> source
        , out bool value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default output.
        value = default;
        detail = ParsingDetail.Ok;

        // Check length.
        if (source.Length == 1)
        {
            // If Y or N, then return, as it's ok
            // Can't overlap with parse details.
            if (source[0] == UpperCaseAsciiCharacterByte.Y)
            {
                // Set output and return.
                value = true;
                return true;
            }
            if (source[0] == UpperCaseAsciiCharacterByte.N)
                // Already defaulted to false, bail
                return true;
        }

        // Set parsing detail.
        if (parseDetail)
            detail = source.ParseDetail();

        // Return.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySpan<byte> source
        , out bool value
        , out ParsingDetail detail
    ) => source.TryParseBoolean(out value, true, out detail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySpan<byte> source
        , out bool value
    ) => source.TryParseBoolean(out value, false, out _);

    #endregion

    #region Integer parsing

    /// <remarks>This method will not throw (but rather, fail) if the value represented by <paramref name="source"/>
    /// is a valid number, but cannot fit in the space of an <see cref="int"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySpan<byte> source
        , out int value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default output.
        detail = ParsingDetail.Ok;

        // If parsed and all the bytes were good, bail.
        if (
            Utf8Parser.TryParse(source, out value, out int bytesConsumed)
            && bytesConsumed == source.Length
        )
            return true;

        // Set parsing detail.
        if (parseDetail)
            detail = source.ParseDetail();

        // Return.
        return false;
    }

    /// <remarks>This method will not throw (but rather, fail) if the value represented by <paramref name="source"/>
    /// is a valid number, but cannot fit in the space of a <see cref="long"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySpan<byte> source
        , out long value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default output.
        detail = ParsingDetail.Ok;

        // If parsed and all the bytes were good, bail.
        if (
            Utf8Parser.TryParse(source, out value, out int bytesConsumed)
            && bytesConsumed == source.Length
        )
            return true;

        // Set parsing detail.
        if (parseDetail)
            detail = source.ParseDetail();

        // Return.
        return false;
    }

    /*
     * The following approaches were taken to parse bigintegers:
     * 
     * - Convert the bytes to a span of characters and then use BigInteger parsing to handle
     * - Convert chunks into 64 bit integers and then update a BigInteger instance appropriately
     * 
     * https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/src/Ofl.Bloomberg.Parsing/ByteParser.cs?at=54222bcdb55787cc691c1fcfecdfb4c0c4a9de61#616,650
     * 
     * In benchmarking them:
     * 
     * https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/benchmarks/Ofl.Bloomberg.Parsing.Benchmarks/ByteParserParseIntegerBigIntegerBenchmarks.cs?at=54222bcdb55787cc691c1fcfecdfb4c0c4a9de61
     * 

|                                   Method |    Input |      Mean |     Error |    StdDev |   Gen0 | Allocated |
|----------------------------------------- |--------- |----------:|----------:|----------:|-------:|----------:|
| TryParseIntegerBigIntegerByStringParsing |  Byte[1] | 133.38 ns |  2.656 ns |  4.924 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[1] |  56.22 ns |  1.150 ns |  1.535 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[2] | 129.80 ns |  2.642 ns |  5.090 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[2] |  56.27 ns |  1.159 ns |  1.999 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[3] | 140.72 ns |  2.815 ns |  3.758 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[3] | 140.71 ns |  2.721 ns |  2.272 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[4] | 141.62 ns |  2.838 ns |  5.045 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[4] | 164.92 ns |  3.346 ns |  5.498 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[5] | 147.84 ns |  3.002 ns |  4.400 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[5] | 159.45 ns |  3.157 ns |  4.321 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[6] | 178.87 ns |  3.593 ns |  4.138 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[6] | 181.31 ns |  3.640 ns |  4.733 ns |      - |         - |
| TryParseIntegerBigIntegerByStringParsing |  Byte[7] | 187.66 ns |  3.769 ns |  5.524 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[7] | 237.64 ns |  4.724 ns |  6.306 ns | 0.0048 |      64 B |
| TryParseIntegerBigIntegerByStringParsing |  Byte[8] | 174.01 ns |  3.515 ns |  3.452 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[8] | 239.38 ns |  4.792 ns |  8.882 ns | 0.0048 |      64 B |
| TryParseIntegerBigIntegerByStringParsing |  Byte[9] | 177.29 ns |  3.304 ns |  2.759 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan |  Byte[9] | 229.68 ns |  4.542 ns |  4.461 ns | 0.0048 |      64 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[10] | 205.33 ns |  4.028 ns |  4.136 ns | 0.0079 |     104 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[10] | 267.93 ns |  5.405 ns |  6.224 ns | 0.0048 |      64 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[11] | 214.75 ns |  4.209 ns |  6.676 ns | 0.0105 |     136 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[11] |        NA |        NA |        NA |      - |         - |
| TryParseIntegerBigIntegerByStringParsing | Byte[12] | 229.42 ns |  4.578 ns |  6.112 ns | 0.0105 |     136 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[12] | 357.04 ns |  6.422 ns |  8.122 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[13] | 244.25 ns |  4.910 ns | 11.952 ns | 0.0105 |     136 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[13] | 387.59 ns |  7.638 ns |  9.381 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[14] | 256.07 ns |  5.089 ns |  8.071 ns | 0.0105 |     136 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[14] | 362.27 ns |  7.040 ns | 10.538 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[15] | 256.37 ns |  5.153 ns |  8.466 ns | 0.0105 |     136 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[15] | 362.08 ns |  7.201 ns | 12.612 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[16] | 307.81 ns |  6.088 ns |  8.924 ns | 0.0181 |     240 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[16] | 399.11 ns |  7.397 ns |  8.806 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[17] | 319.90 ns |  6.353 ns |  9.890 ns | 0.0181 |     240 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[17] | 348.72 ns |  7.044 ns | 11.173 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[18] | 322.60 ns |  6.498 ns | 11.033 ns | 0.0181 |     240 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[18] | 375.96 ns |  7.504 ns |  8.030 ns | 0.0134 |     176 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[19] | 336.60 ns |  6.234 ns |  6.123 ns | 0.0181 |     240 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[19] | 501.52 ns |  8.724 ns | 14.088 ns | 0.0191 |     248 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[20] | 350.48 ns |  7.045 ns | 12.703 ns | 0.0181 |     240 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[20] | 577.24 ns |  9.558 ns |  7.981 ns | 0.0191 |     248 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[21] | 358.06 ns |  6.972 ns | 16.434 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[21] | 495.56 ns |  7.992 ns | 10.107 ns | 0.0191 |     256 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[22] | 360.27 ns |  7.143 ns | 11.735 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[22] | 536.88 ns |  8.989 ns |  9.231 ns | 0.0200 |     264 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[23] | 371.35 ns |  7.219 ns | 10.581 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[23] | 546.40 ns | 10.356 ns | 22.070 ns | 0.0210 |     272 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[24] | 375.40 ns |  7.301 ns |  9.747 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[24] | 561.93 ns | 11.277 ns | 16.530 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[25] | 386.23 ns |  7.613 ns | 13.334 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[25] | 521.35 ns | 10.103 ns | 10.810 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[26] | 391.85 ns |  7.859 ns | 14.761 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[26] | 542.21 ns |  9.807 ns | 15.268 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[27] | 405.00 ns |  6.717 ns |  7.466 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[27] | 602.41 ns | 11.958 ns | 16.764 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[28] | 424.36 ns |  8.176 ns |  9.415 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[28] | 565.89 ns | 10.720 ns | 10.528 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[29] | 433.22 ns |  8.547 ns | 12.793 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[29] |        NA |        NA |        NA |      - |         - |
| TryParseIntegerBigIntegerByStringParsing | Byte[30] | 440.38 ns |  8.834 ns | 16.374 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[30] | 578.90 ns |  8.974 ns |  8.395 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[31] | 454.47 ns |  9.095 ns | 14.159 ns | 0.0191 |     248 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[31] | 620.86 ns | 12.230 ns | 13.086 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[32] | 490.08 ns |  9.720 ns | 20.504 ns | 0.0296 |     384 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[32] | 627.67 ns | 12.426 ns | 22.087 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[33] | 529.64 ns | 10.495 ns | 19.190 ns | 0.0296 |     384 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[33] | 575.49 ns | 11.271 ns | 15.428 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[34] | 536.52 ns | 10.583 ns | 19.617 ns | 0.0296 |     384 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[34] | 594.41 ns |  9.966 ns |  7.781 ns | 0.0219 |     296 B |
| TryParseIntegerBigIntegerByStringParsing | Byte[35] | 563.08 ns | 11.268 ns | 14.250 ns | 0.0296 |     384 B |
|    TryParseIntegerBigIntegerByManualScan | Byte[35] | 607.29 ns | 11.768 ns | 12.085 ns | 0.0219 |     296 B |

     * The following is apparent:
     * 
     * - There is a slight memory overhead for the int64 method compared to string parsing (which is a little surprising)
     * - The string parsing is faster in every test except for 1 or two characters
     * 
     * As a result, taking the string parsing.  One thing to explore would be to still chunk in 8 byte chunks, working
     * from the back of the string and building up the big integer instead of working back, as we may be over allocating.
     * 
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default output.
        value = BigInteger.Zero;
        detail = ParsingDetail.Ok;

        // Get the characters needed.
        Span<char> chars = stackalloc char[Encoding.ASCII.GetCharCount(source)];
        var decoded = Encoding.ASCII.GetChars(source, chars);

        // If not decoded, bail.  There's nothing in the parse details that could
        // override, so we're ok here.
        if (decoded != source.Length)
            return false;

        // If we are successful in parsing, we can return, since details are
        // set to ok.
        if (BigInteger.TryParse(chars, out value))
            return true;

        // Set parsing detail.
        if (parseDetail)
            detail = source.ParseDetail();

        // Return.
        return false;
    }

    /// <inheritdoc cref="TryParseIntegerInt32(in ReadOnlySpan{byte}, out int, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySpan<byte> source
        , out int value
        , out ParsingDetail detail
    ) => source.TryParseIntegerInt32(out value, true, out detail);

    /// <inheritdoc cref="TryParseIntegerInt32(in ReadOnlySpan{byte}, out int, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySpan<byte> source
        , out int value
    ) => source.TryParseIntegerInt32(out value, false, out _);

    /// <inheritdoc cref="TryParseIntegerInt64(in ReadOnlySpan{byte}, out long, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySpan<byte> source
        , out long value
        , out ParsingDetail detail
    ) => source.TryParseIntegerInt64(out value, true, out detail);

    /// <inheritdoc cref="TryParseIntegerInt64(in ReadOnlySpan{byte}, out long, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySpan<byte> source
        , out long value
    ) => source.TryParseIntegerInt64(out value, false, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger value
        , out ParsingDetail detail
    ) => source.TryParseIntegerBigInteger(out value, true, out detail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger value
    ) => source.TryParseIntegerBigInteger(out value, false, out _);

    #endregion

    #region Real parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySpan<byte> source
        , out decimal value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default, we use no matter what.
        detail = ParsingDetail.Ok;

        // Parsing of the formats can be done first because
        // it's not possible for the parse result detail
        // to exist in those formats.
        if (
            Utf8Parser.TryParse(source, out value, out var bytesConsumed)
            && bytesConsumed == source.Length
        )
            // Assume the parse result detail is ok.
            return true;

        // If parsing the parse result detail.
        if (parseDetail)
            // Get the parse result detail.
            detail = source.ParseDetail();

        // Bail.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySpan<byte> source
        , out decimal value
        , out ParsingDetail detail
    ) => source.TryParseReal(out value, true, out detail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySpan<byte> source
        , out decimal value
    ) => source.TryParseReal(out value, false, out _);

    #endregion

    #region Price parsing.

    /// <remarks>Special Bloomberg fractions (referenced in
    /// <seealso href="https://data.bloomberg.com/docs/data-license/#9-8-data-types"/> under
    /// "Price") are not currently supported.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParsePrice(
        this in ReadOnlySpan<byte> source
        , out decimal value
        , out ParsingDetail detail
    ) => source.TryParseReal(out value, true, out detail);

    /// <inheritdoc cref="TryParsePrice(in ReadOnlySpan{byte}, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParsePrice(
        this in ReadOnlySpan<byte> source
        , out decimal value
    ) => source.TryParseReal(out value, false, out _);

    #endregion

    #region Integer/real parsing.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySpan<byte> source
        , out int integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default.
        integer = default;

        // The real result.
        var realResult = source.TryParseReal(out real, parseDetail, out detail);

        // If it fails, and the parse detail result is set
        // to something *other* than ok, we can bail.
        if (!realResult && detail != ParsingDetail.Ok)
            return ParseIntegerRealResult.None;

        // Parse the integer now as well.  We do *not* need parsing
        // detail at this point:
        // - If it was true and something other than ok, then that was handled 👆
        // - If it was false, we're not going to call it again here anyways.
        // So always pass false.
        var integerResult = source.TryParseIntegerInt32(out integer, false, out _);

        // Set the result and return.
        return (realResult ? ParseIntegerRealResult.Real : ParseIntegerRealResult.None)
            | (integerResult ? ParseIntegerRealResult.Integer : ParseIntegerRealResult.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySpan<byte> source
        , out long integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default.
        integer = default;

        // The real result.
        var realResult = source.TryParseReal(out real, parseDetail, out detail);

        // If it fails, and the parse detail result is set
        // to something *other* than ok, we can bail.
        if (!realResult && detail != ParsingDetail.Ok)
            return ParseIntegerRealResult.None;

        // Parse the integer now as well.  We do *not* need parsing
        // detail at this point:
        // - If it was true and something other than ok, then that was handled 👆
        // - If it was false, we're not going to call it again here anyways.
        // So always pass false.
        var integerResult = source.TryParseIntegerInt64(out integer, false, out _);

        // Set the result and return.
        return (realResult ? ParseIntegerRealResult.Real : ParseIntegerRealResult.None)
            | (integerResult ? ParseIntegerRealResult.Integer : ParseIntegerRealResult.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // Default.
        integer = default;

        // The real result.
        var realResult = source.TryParseReal(out real, parseDetail, out detail);

        // If it fails, and the parse detail result is set
        // to something *other* than ok, we can bail.
        if (!realResult && detail != ParsingDetail.Ok)
            return ParseIntegerRealResult.None;

        // Parse the integer now as well.  We do *not* need parsing
        // detail at this point:
        // - If it was true and something other than ok, then that was handled 👆
        // - If it was false, we're not going to call it again here anyways.
        // So always pass false.
        var integerResult = source.TryParseIntegerBigInteger(out integer, false, out _);

        // Set the result and return.
        return (realResult ? ParseIntegerRealResult.Real : ParseIntegerRealResult.None)
            | (integerResult ? ParseIntegerRealResult.Integer : ParseIntegerRealResult.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySpan<byte> source
        , out int integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealInt32(
        out integer
        , out real
        , true
        , out detail
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySpan<byte> source
        , out int integer
        , out decimal real
    ) => source.TryParseIntegerRealInt32(
        out integer
        , out real
        , false
        , out _
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySpan<byte> source
        , out long integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealInt64(
        out integer
        , out real
        , true
        , out detail
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySpan<byte> source
        , out long integer
        , out decimal real
    ) => source.TryParseIntegerRealInt64(
        out integer
        , out real
        , false
        , out _
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealBigInteger(
        out integer
        , out real
        , true
        , out detail
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySpan<byte> source
        , out BigInteger integer
        , out decimal real
    ) => source.TryParseIntegerRealBigInteger(
        out integer
        , out real
        , false
        , out _
    );

    #endregion
}

using Riok.Mapperly.Abstractions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.Fields.DataTransferObjects;

[Mapper]
internal static partial class FieldDataTransferObjectToFieldMapper
{
    #region Helpers

    private static bool? NullableStringToNullableBoolean(this string? s) => s?.Trim() switch {
        "Y" => true
        , "y" => true
        , "N" => false
        , "n" => false
        , "T" => true
        , "t" => true
        , "F" => false
        , "f" => false
        , _ => null
    };

    private static bool NonNullableStringToBoolean(this string s) => 
        s.NullableStringToNullableBoolean()!.Value;

    private static DateOnly NonNullableStringToDateOnly(this string s) => DateOnly.ParseExact(
        s
        , "yyyyMMdd"
        , CultureInfo.InvariantCulture
        , DateTimeStyles.AllowWhiteSpaces
    );

    private static int NonNullableStringToNullableInt32(this string s) =>
        s.NullableStringToNullableInt32()!.Value;

    private static int? NullableStringToNullableInt32(this string? s) => int.TryParse(
        s
        , NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowExponent
        , CultureInfo.InvariantCulture
        , out var n
    )
        ? n
        : null;

    private static long NonNullableStringToNullableInt64(this string s) =>
        s.NullableStringToNullableInt64()!.Value;

    private static long? NullableStringToNullableInt64(this string? s) => long.TryParse(
        s
        , NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowExponent
        , CultureInfo.InvariantCulture
        , out var n
    )
        ? n
        : null;

    private static decimal? NullableStringToNullableDecimal(this string? s) => decimal.TryParse(
        s
        , NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowExponent
        , CultureInfo.InvariantCulture
        , out var n
    )
        ? n
        : null;

    private static Uri? NullableStringToUri(this string? s) => string.IsNullOrWhiteSpace(s)
        ? default
        : new Uri(s);

    private static partial Field MapFieldDataTransferObjectToField(this FieldDataTransferObject dto);

    private static bool? TrueIfEquals(this string? s, string value) =>
        s == value ? true : null;

    #endregion

    #region Public methods.

    public static Field ToField(this FieldDataTransferObject dto)
    {
        // Call the overload.
        var f = dto.MapFieldDataTransferObjectToField();

        // Post mapping.
        return f with
        {
            Comdty = dto.Comdty.TrueIfEquals(nameof(f.Comdty))
            , Equity = dto.Equity.TrueIfEquals(nameof(f.Equity))
            , Muni = dto.Muni.TrueIfEquals(nameof(f.Muni))
            , Pfd = dto.Pfd.TrueIfEquals(nameof(f.Pfd))
            , MMkt = dto.MMkt.TrueIfEquals("M-Mkt")
            , Govt = dto.Govt.TrueIfEquals(nameof(f.Govt))
            , Corp = dto.Corp.TrueIfEquals(nameof(f.Corp))
            , Index = dto.Index.TrueIfEquals(nameof(f.Index))
            , Curncy = dto.Curncy.TrueIfEquals(nameof(f.Curncy))
            , Mtge = dto.Mtge.TrueIfEquals(nameof(f.Mtge))
            , BackOffice = dto.BackOffice.TrueIfEquals("Back Office")
            , ExtendedBackOffice = dto.BackOffice.TrueIfEquals("Extended Back Office")
            , Bval = dto.Bval.TrueIfEquals("BVAL")
            , BvalBlocked = dto.BvalBlocked.TrueIfEquals("BVAL Blocked")
            , GetFundamentals = dto.GetFundamentals.TrueIfEquals("Getfundamentals")
            , GetHistory = dto.GetHistory.TrueIfEquals("Gethistory")
            , GetCompany = dto.GetCompany.TrueIfEquals("Getcompany")
            , DsBvalMetered = dto.DsBvalMetered.TrueIfEquals("DS BVAL Metered")
        };
    }

    #endregion
}

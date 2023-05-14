namespace Ofl.Bloomberg.Parsing;

internal static class ParsingDetailValue
{
    // N.A.
    public static readonly byte[] DataMissing = "N.A."u8.ToArray();

    // N.D.
    public static readonly byte[] NotDownloadable = "N.D."u8.ToArray();

    // N.S.
    public static readonly byte[] NotSubscribed = "N.S."u8.ToArray();

    // FLD UNKNOWN
    public static readonly byte[] FieldUnknown = "FLD UNKNOWN"u8.ToArray();
}

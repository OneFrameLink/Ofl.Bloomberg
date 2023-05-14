namespace Ofl.Bloomberg.BackOffice;

// Note: Obvious dangers here of using mutable arrays
// Done in the name of performance (spans wrap arrays effortlessly).
public static class BloombergBackOfficeSection
{
    public static readonly byte[] StartOfFile =
        Constant.Encoding.GetBytes("START-OF-FILE");

    public static readonly byte[] StartOfFields =
        Constant.Encoding.GetBytes("START-OF-FIELDS");

    public static readonly byte[] EndOfFields =
        Constant.Encoding.GetBytes("END-OF-FIELDS");

    public static readonly byte[] StartOfData =
        Constant.Encoding.GetBytes("START-OF-DATA");

    public static readonly byte[] EndOfData =
        Constant.Encoding.GetBytes("END-OF-DATA");

    public static readonly byte[] EndOfFile =
        Constant.Encoding.GetBytes("END-OF-FILE");
}

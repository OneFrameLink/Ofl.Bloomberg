using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ofl.Bloomberg.BackOffice;

public static class Constant
{
    #region Read-only state

    public static readonly byte NewLine = (byte)'\n';

    public static readonly byte KeyValueDelimiter = (byte)'=';

    public static readonly byte Comment = (byte) '#';

    public static readonly byte FieldValueDelimiter = (byte) '|';

    // ASCII as per: https://data.bloomberg.com/docs/data-license/#4-4-1-supported-platforms
    public static readonly Encoding Encoding = Encoding.ASCII;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IgnoreLineProcessing(in ReadOnlySequence<byte> _) { }

    #endregion
}

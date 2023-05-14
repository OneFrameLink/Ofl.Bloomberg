using System.Buffers;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Extensions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<ReadOnlySequence<byte>> ReadLineAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , CancellationToken cancellationToken
    ) => await bloombergBackOfficeReader.ReadToDelimiterAsync(
        Constant.NewLine
        , cancellationToken
    )
    .ConfigureAwait(false);

    #endregion
}

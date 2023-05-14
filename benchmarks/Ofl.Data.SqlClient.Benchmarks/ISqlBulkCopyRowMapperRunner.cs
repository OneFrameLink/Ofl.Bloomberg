using System.Runtime.CompilerServices;

namespace Ofl.Data.SqlClient.Benchmarks;

internal interface ISqlBulkCopyRowMapperRunner : IDisposable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    object? Map(int ordinal);
}

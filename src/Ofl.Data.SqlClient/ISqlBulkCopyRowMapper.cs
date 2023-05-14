namespace Ofl.Data.SqlClient;

public interface ISqlBulkCopyRowMapper<T> : IDisposable
{
    int FieldCount { get; }

    object? Map(
        in T instance,
        int ordinal
    );
}

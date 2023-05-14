namespace Ofl.Data.SqlClient;

public delegate TProperty SqlBulkCopyRowValueMapper<T, out TProperty>(
    in T instance
);

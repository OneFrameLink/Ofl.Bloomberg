namespace Ofl.Data.SqlClient;

public delegate TProperty SqlBulkCopyRowValueAccessor<T, TProperty>(in T t);
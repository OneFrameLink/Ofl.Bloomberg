namespace Ofl.Data.SqlClient;

/// <summary>Model for duck typing that row value mappers
/// need to implement.</summary>
internal interface ISqlBulkCopyRowValueMapper<T, out TProperty>
{
    TProperty Map(in T instance);
}

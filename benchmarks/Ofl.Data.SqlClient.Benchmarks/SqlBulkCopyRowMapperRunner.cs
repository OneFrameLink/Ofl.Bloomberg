using Ofl.Data.SqlClient.Tests;
using System.Runtime.CompilerServices;

namespace Ofl.Data.SqlClient.Benchmarks;

internal class SqlBulkCopyRowMapperRunner<T> : ISqlBulkCopyRowMapperRunner
    where T : new()
{
    #region Instance, read-only state

    private readonly ISqlBulkCopyRowMapper<T> _mapper;

    private readonly T _instance = new();

    #endregion

    #region Constructor

    public SqlBulkCopyRowMapperRunner(
        IEnumerable<int> indices
        , bool fromDelegate
    )
    {
        // We can create our mappings from that.
        var mappings = indices
            .Select(i => {
                // Create the column.
                var column = new BenchmarkDbColumn($"c{i}", i);

                // Create the mapping and return.
                return fromDelegate
                    ? SqlBulkCopyMapperColumnMapping.FromDelegate<T, int>(
                        column
                        , (in T _) => default!
                    )
                    : SqlBulkCopyMapperColumnMapping.FromDuckTypedObjectWithMapMethod(
                        column
                        , new SingleValueDuckTypeMapper<T, int>(default!)
                    );
            })
            .ToArray()
            .AsReadOnly();

        // Create the mapper.
        _mapper = mappings.ToArray().CreateSqlBulkCopyMapper<T>();
    }

    #endregion

    #region ISqlBulkCopyRowMapperRunner implementation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? Map(int ordinal) => _mapper.Map(in _instance, ordinal);

    #endregion

    #region IDisposable implementation

    public void Dispose() => _mapper.Dispose();

    #endregion
}

using System.Runtime.CompilerServices;

namespace Ofl.Data.SqlClient;

public class SqlBulkCopyRowValueDelegateMapper<T, TProperty>
{
    #region Instance, read-only state

    private readonly SqlBulkCopyRowValueAccessor<T, TProperty> _mapper;

    #endregion

    #region Constructor

    public SqlBulkCopyRowValueDelegateMapper(
        SqlBulkCopyRowValueAccessor<T, TProperty> mapper
    )
    {
        // Assign values.
        _mapper = mapper;
    }

    #endregion

    #region ISqlBulkCopyRowValueMapper<T, TProperty> duck type implementation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TProperty Map(in T instance) => _mapper(in instance);

    #endregion
}

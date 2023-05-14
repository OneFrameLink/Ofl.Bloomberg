using System.Data.Common;

namespace Ofl.Data.SqlClient;

public readonly struct SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    internal readonly object RowValueAccessor;

    public readonly DbColumn Column;

    #endregion

    #region Constructors/factories.

    private SqlBulkCopyMapperColumnMapping(
        DbColumn column
        , object rowValueMapper
    )
    {
        // Assign values.
        Column = column;
        RowValueAccessor = rowValueMapper;
    }

    public static SqlBulkCopyMapperColumnMapping FromDuckTypedObjectWithMapMethod(
        DbColumn column
        , object accessor
    ) =>
        // TODO: Consider lifting the check for public class and method here
        // and not performing the check when creating the type (at least for duck
        // methods, the delegate doesn't have this problem).
        new(column, accessor);

    public static SqlBulkCopyMapperColumnMapping FromDelegate<T, TParameter>(
        DbColumn column
        , SqlBulkCopyRowValueAccessor<T, TParameter> accessor
    ) => new(column, accessor);

    #endregion
}

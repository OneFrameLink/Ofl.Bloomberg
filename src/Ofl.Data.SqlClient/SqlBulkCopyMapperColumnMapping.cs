using System.Data.Common;
using System.Linq.Expressions;

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

    // TODO: Check to see if we can improve delegate performance
    // when the method and target is accessible by the generated
    // type (so we don't need the overhead of the delegate at all)
    public static SqlBulkCopyMapperColumnMapping FromDelegate<T, TParameter>(
        DbColumn column
        , SqlBulkCopyRowValueAccessor<T, TParameter> accessor
    ) => new(column, accessor);

    // We can parse the expression by going to the field/property
    // directly, this should allow us to get the best performance
    // given that we don't need the overhead of a call to the map
    // method or a delegate.
    public static SqlBulkCopyMapperColumnMapping FromExpression<T>(
        DbColumn column
        , Expression<Func<T, object>> accessor
    ) => new(column, accessor);

    #endregion
}

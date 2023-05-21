namespace Ofl.Data.SqlClient.Tests;

public class InstanceValueMapper<T, TProperty>
{
    #region Instance, read-only state

    private readonly SqlBulkCopyRowValueAccessor<T, TProperty> _accessor;

    #endregion

    #region Constructor

    public InstanceValueMapper(
        SqlBulkCopyRowValueAccessor<T, TProperty> accessor
    )
    {
        // Assign values.
        _accessor = accessor;
    }

    #endregion

    #region Helpers

    public TProperty Map(in T instance) => _accessor(in instance);

    #endregion
}

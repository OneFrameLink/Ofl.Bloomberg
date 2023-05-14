namespace Ofl.Data.SqlClient.Tests;

public class DuckTypedMapper<T, TProperty>
{
    #region Instance, read-only state

    private readonly TProperty _value;

    #endregion

    #region Constructor

    public DuckTypedMapper(
        TProperty value
    )
    {
        // Assign values.
        _value = value;
    }

    #endregion

    #region Mapping method

    public TProperty Map(in T _) => _value;

    #endregion
}

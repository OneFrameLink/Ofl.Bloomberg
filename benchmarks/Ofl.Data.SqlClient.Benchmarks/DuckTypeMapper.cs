namespace Ofl.Data.SqlClient.Benchmarks;

public class DuckTypeMapper<T, TProperty>
{
    #region Instance, read-only state

    private readonly TProperty _value;

    #endregion

    #region Constructor

    public DuckTypeMapper(TProperty value)
    {
        // Assign values.
        _value = value;
    }

    #endregion

    #region Duck typed mapping method

    public TProperty Map(in T _) => _value;

    #endregion
}

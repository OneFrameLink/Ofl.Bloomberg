namespace Ofl.Data.SqlClient.Benchmarks;

public class SingleValueDuckTypeMapper<T, TProperty>
{
    #region Instance, read-only state

    private readonly TProperty _value;

    #endregion

    #region Constructor

    public SingleValueDuckTypeMapper(TProperty value)
    {
        // Assign values.
        _value = value;
    }

    #endregion

    #region Duck typed mapping method

    public TProperty Map(in T _) => _value;

    #endregion
}

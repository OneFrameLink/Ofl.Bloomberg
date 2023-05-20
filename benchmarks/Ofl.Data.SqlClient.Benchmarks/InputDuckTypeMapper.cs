namespace Ofl.Data.SqlClient.Benchmarks;

public class InputDuckTypeMapper<T>
{
    #region Duck typed mapping method

    public T Map(in Input<T> t) => t.Value;

    #endregion
}

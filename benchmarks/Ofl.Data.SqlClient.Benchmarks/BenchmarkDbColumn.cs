using System.Data.Common;

namespace Ofl.Data.SqlClient.Tests;

internal class BenchmarkDbColumn : DbColumn
{
    public BenchmarkDbColumn(
        string name
        , int ordinal
    )
    {
        // Assign values.
        ColumnName = name;
        ColumnOrdinal = ordinal;
    }
}
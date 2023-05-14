using System.Data.Common;

namespace Ofl.Data.SqlClient.Tests;

public class TestDbColumn : DbColumn
{
    public TestDbColumn(
        string name
        , int ordinal
    )
    {
        // Assign values.
        ColumnName = name;
        ColumnOrdinal = ordinal;
    }
}
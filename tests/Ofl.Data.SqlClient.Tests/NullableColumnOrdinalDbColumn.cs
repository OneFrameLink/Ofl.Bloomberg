using System.Data.Common;

namespace Ofl.Data.SqlClient.Tests;

public class NullableColumnOrdinalDbColumn : DbColumn
{
    public NullableColumnOrdinalDbColumn(
        string name
    )
    {
        // Assign values.
        ColumnName = name;
    }
}
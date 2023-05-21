using Ofl.Core.Reflection.Emit;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

internal sealed class NullSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Constructors/factories.

    public NullSqlBulkCopyMapperColumnMapping(
        int ordinal
        , Type inputType
    ) : base(
        ordinal
        , default
        , default
        , inputType
        , typeof(object)
    )
    { }

    #endregion

    #region Overrides

    internal protected override void WriteGetValueIL(
        ILGenerator il
        , FieldBuilder? rowValueAccessorFieldBuilder
    ) => il.PushNull();

    #endregion
}

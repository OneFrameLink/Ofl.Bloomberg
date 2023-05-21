using Ofl.Core.Reflection.Emit;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

// NOTE: Yes, constant may be a misnomer, but it's constant throughout
// the life of the mapper that is generated.
internal sealed class ConstantSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Constructors/factories.

    public ConstantSqlBulkCopyMapperColumnMapping(
        int ordinal
        , Type inputType
        , object value
    ) : base(
        ordinal
        , value
        , typeof(object)
        , inputType
        , typeof(object)
    )
    { }

    #endregion

    #region Overrides

    internal protected override void WriteGetValueIL(
        ILGenerator il
        , FieldBuilder? rowValueAccessorFieldBuilder
    )
    {
        // The field builder must exist.
        ArgumentNullException.ThrowIfNull(
            rowValueAccessorFieldBuilder
            , nameof(rowValueAccessorFieldBuilder)
        );

        // Push this.
        il.PushThis();

        // Load the field.
        il.Emit(OpCodes.Ldfld, rowValueAccessorFieldBuilder);
    }

    #endregion
}

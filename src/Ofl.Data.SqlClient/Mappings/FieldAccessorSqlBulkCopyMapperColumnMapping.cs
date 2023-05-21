using Ofl.Core.Reflection.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

internal sealed class FieldAccessorSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    private readonly FieldInfo _fieldInfo;

    #endregion

    #region Constructor.

    public FieldAccessorSqlBulkCopyMapperColumnMapping(
        int ordinal
        , Type inputType
        , FieldInfo fieldInfo
    ) : base(
        ordinal
        , default
        , default
        , inputType
        , fieldInfo.FieldType
    )
    {
        // Assign values.
        _fieldInfo = fieldInfo;
    }

    #endregion

    #region Overrides

    internal protected override void WriteGetValueIL(
        ILGenerator il
        , FieldBuilder? rowValueAccessorFieldBuilder
    )
    {
        // Load the first parameter, it's an address already.
        il.PushArgument(1);

        // If this is a reference type, then
        // dereference.
        if (!InputType.IsValueType)
            il.Emit(OpCodes.Ldind_Ref);

        // Load the field.
        il.Emit(OpCodes.Ldfld, _fieldInfo);
    }

    #endregion
}

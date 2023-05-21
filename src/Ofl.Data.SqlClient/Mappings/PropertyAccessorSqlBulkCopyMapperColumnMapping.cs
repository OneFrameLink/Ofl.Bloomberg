using Ofl.Core.Reflection.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

internal sealed class PropertyAccessorSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    private readonly MethodInfo _methodInfo;

    #endregion

    #region Constructor.

    public PropertyAccessorSqlBulkCopyMapperColumnMapping(
        int ordinal
        , Type inputType
        , MethodInfo methodInfo
    ) : base(
        ordinal
        , default
        , default
        , inputType
        , methodInfo.ReturnType
    )
    {
        // Assign values.
        _methodInfo = methodInfo;
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

        // Deref if the input is a reference type.
        // TODO: Separate out into two different
        // implementations, one for value type, one
        // for reference types (so value types are passed
        // in as ref/in while reference types are not).
        if (!InputType.IsValueType)
            il.Emit(OpCodes.Ldind_Ref);

        // Call the getter on the property.
        il.Emit(OpCodes.Call, _methodInfo);
    }

    #endregion
}

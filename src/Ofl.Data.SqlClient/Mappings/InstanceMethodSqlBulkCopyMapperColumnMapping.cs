using Ofl.Core.Reflection.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

internal sealed class InstanceMethodSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    private readonly MethodInfo _methodInfo;

    #endregion

    #region Constructor.

    public InstanceMethodSqlBulkCopyMapperColumnMapping(
        int ordinal
        , object mapperFieldValue
        , Type inputType
        , MethodInfo methodInfo
    ) : base(
        ordinal
        , mapperFieldValue
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
        // If the field builder is null, throw.
        ArgumentNullException.ThrowIfNull(
            rowValueAccessorFieldBuilder
            , nameof(rowValueAccessorFieldBuilder)
        );

        // Load this.
        il.PushThis();

        // Is this a value type?  If so, push the address of the
        // value type onto the stack.
        var opCode = Field!.Value.FieldType.IsValueType
            ? OpCodes.Ldflda
            : OpCodes.Ldfld;

        // Load the field.
        il.Emit(opCode, rowValueAccessorFieldBuilder);

        // Load the first parameter, this is address
        // of the instance of T (no need to get it's address)
        il.PushArgument(1);

        // Call the method
        il.Emit(OpCodes.Call, _methodInfo);
    }

    #endregion
}

using Ofl.Core.Reflection.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient.Mappings;

internal sealed class StaticMethodSqlBulkCopyMapperColumnMapping : SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    private readonly MethodInfo _methodInfo;

    #endregion

    #region Constructors/factories.

    public StaticMethodSqlBulkCopyMapperColumnMapping(
        int ordinal
        , Type inputType
        , MethodInfo methodInfo
    ) : base(ordinal, default, inputType, methodInfo.ReturnType)
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
        // Load the first parameter, this is address
        // of the instance of T (no need to get it's address)
        il.PushArgument(1);

        // Call the method, that's it.
        il.Emit(OpCodes.Call, _methodInfo);
    }

    #endregion
}

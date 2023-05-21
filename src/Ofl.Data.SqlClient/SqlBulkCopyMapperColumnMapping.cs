using Ofl.Data.SqlClient.Mappings;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient;

public abstract class SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    internal readonly object? RowValueAccessor;

    public readonly int Ordinal;

    internal readonly Type InputType;

    internal readonly Type ReturnType;

    #endregion

    #region Constructors.

    protected SqlBulkCopyMapperColumnMapping(
        int ordinal
        , object? rowValueMapper
        , Type inputType
        , Type returnType
    )
    {
        // Assign values.
        Ordinal = ordinal;
        RowValueAccessor = rowValueMapper;
        InputType = inputType;
        ReturnType = returnType;
    }

    #endregion

    #region Overrides

    internal protected abstract void WriteGetValueIL(
        ILGenerator il
        , FieldBuilder? rowValueAccessorFieldBuilder
    );

    #endregion

    #region Factories

    public static SqlBulkCopyMapperColumnMapping FromDuckTypedObjectWithMapMethod<T>(
        int ordinal
        , object accessor
    ) => FromDuckTypedObjectWithMapMethod(
        ordinal
        , accessor
        , typeof(T)
    );

    public static SqlBulkCopyMapperColumnMapping FromDuckTypedObjectWithMapMethod(
        int ordinal
        , object accessor
        , Type inputType
    )
    {
        // Format the input type.
        inputType = inputType.IsByRef
            ? inputType
            : inputType.MakeByRefType();

        // The map method name.  If it is a delegate then we
        // want the Invoke method.
        const string mapMethodName = 
            nameof(ISqlBulkCopyRowValueMapper<object, object>.Map);

        // There must be a Map method that takes an in input of
        // type T and return any type.
        // Cycle through, as we want to be as open as possible.
        var mapMethodInfos = accessor
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == mapMethodName);

        // The singular map method info.
        MethodInfo mapMethodInfo = default!;

        // Cycle.
        foreach (var localMapMethodInfo in mapMethodInfos)
        {
            // Get the parameters.
            var parameters = localMapMethodInfo.GetParameters();

            // Is there one?  If not, continue.
            if (parameters.Length != 1) continue;

            // Get the parameter.
            var parameter = parameters.Single();

            // Is the first parameter assignable from the input type?
            if (!parameter.ParameterType.IsAssignableFrom(inputType))
                continue;

            // Is it an in parameter?  If not, continue.
            // TODO: consider if ref is ok here as well.
            if (!parameter.IsIn) continue;

            // Assign the method info.
            // If already assigned, throw.
            if (mapMethodInfo is not null)
                throw new ArgumentException(
                    $"Could not find a single public instance method named {mapMethodName} " 
                    + $"with a single parameter of type {inputType.FullName} on the "
                    + $"{nameof(accessor)} parameter."
                    , nameof(accessor)
                );

            // Assign.
            mapMethodInfo = localMapMethodInfo;
        }

        // If there are no methods, throw.
        if (mapMethodInfo is null)
            throw new ArgumentException(
                $"Could not find a single public instance method named {mapMethodName} "
                + $"with a single parameter of type {inputType.FullName} on the "
                + $"{nameof(accessor)} parameter."
                , nameof(accessor)
            );

        // Return an instance method invoker.
        return new InstanceMethodSqlBulkCopyMapperColumnMapping(
            ordinal
            , accessor
            , inputType
            , mapMethodInfo
        );
    }

    public static SqlBulkCopyMapperColumnMapping FromDelegate<T, TParameter>(
        int ordinal
        , SqlBulkCopyRowValueAccessor<T, TParameter> accessor
    )
    {
        // The input type.
        var inputType = typeof(T).MakeByRefType();

        // If the accessor method is public, we can work with that.
        if (accessor.Method.IsPublic)
        {
            // Is this static?  If so, call specifically.
            if (accessor.Target is null)
                // Call the method.
                return new StaticMethodSqlBulkCopyMapperColumnMapping(
                    ordinal
                    , inputType
                    , accessor.Method
                );

            // There's an instance.
            // If the type is fully accessible (check nested all the way up)
            // then we can just pass *that* and call the method directly.
            if (accessor.Target.GetType().IsFullyPublic())
                return new InstanceMethodSqlBulkCopyMapperColumnMapping(
                    ordinal
                    , accessor.Target
                    , inputType
                    , accessor.Method
                );
        }

        // Nothing to work with, so just call the instance method accessor
        // since the delegate *is* public and we can call it.
        // First, get the method info for the Invoke method on the delegate
        // itself.
        var invokeMethodInfo = accessor
            .GetType()
            .GetMethod(
                nameof(SqlBulkCopyRowValueAccessor<T, TParameter>.Invoke)
                , BindingFlags.Instance | BindingFlags.Public
            )
            ?? throw new InvalidOperationException(
                "Could not find the public instance method " 
                + $"{nameof(SqlBulkCopyRowValueAccessor<T, TParameter>.Invoke)} "
                + $"on the type {typeof(SqlBulkCopyRowValueAccessor<T, TParameter>).FullName}."
            );

        // Pass to the instance invoker mapping.
        return new InstanceMethodSqlBulkCopyMapperColumnMapping(
            ordinal
            , accessor
            , inputType
            , invokeMethodInfo
        );
    }

    #endregion
}

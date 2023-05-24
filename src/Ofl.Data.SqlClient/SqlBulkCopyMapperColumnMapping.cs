using Ofl.Data.SqlClient.Mappings;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient;

public abstract class SqlBulkCopyMapperColumnMapping
{
    #region Instance, read-only state

    internal readonly MapperField? Field;

    public readonly int Ordinal;

    internal readonly Type InputType;

    internal readonly Type ReturnType;

    #endregion

    #region Constructors.

    protected SqlBulkCopyMapperColumnMapping(
        int ordinal
        , object? mapperFieldValue
        , Type? mapperFieldType
        , Type inputType
        , Type returnType
    )
    {
        // Check the input type.
        CheckInputType(inputType);

        // Assign values.
        Ordinal = ordinal;
        Field = mapperFieldValue is null
            ? null
            : new(
                mapperFieldValue
                , mapperFieldType ?? mapperFieldValue.GetType()
            );
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

    #region Helpers

    private static void CheckInputType(Type inputType)
    {
        // If this is a by ref type, then throw.
        if (inputType.IsByRef)
            throw new ArgumentException(
                $"The {nameof(inputType)} parameter must not be a reference to a type "
                + $"(i.e. {nameof(inputType.IsByRef)} must be false)."
                , nameof(inputType)
            );
    }

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

        // Get the parameter input type.
        var parameterInputType = inputType.MakeByRefType();

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
            if (!parameter.ParameterType.IsAssignableFrom(parameterInputType))
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
        var inputType = typeof(T);

        // If the accessor method is public, we can work with that.
        if (accessor.Method.IsPublic)
        {
            // Is this static?  If so, call specifically.
            if (accessor.Target is null)
            {
                // If the declaring type of the method is fully public
                // then return a static method call.
                if (accessor.Method.DeclaringType?.IsFullyPublic() ?? false)
                    // Call the method.
                    return new StaticMethodSqlBulkCopyMapperColumnMapping(
                        ordinal
                        , inputType
                        , accessor.Method
                    );
            }
            else
            {
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

    public static SqlBulkCopyMapperColumnMapping FromInstanceProperty(
        int ordinal
        , Type inputType
        , string property
    )
    {
        // If input type is not fully public, then throw, since
        // there's nothing we can do.
        if (!inputType.IsFullyPublic())
            throw new ArgumentException(
                $"The {nameof(inputType)} parameter "
                + $"({inputType.FullName}) must be public."
                , nameof(inputType)
            );

        // Get the property.
        var propertyInfo = inputType
            .GetProperty(property, BindingFlags.Instance | BindingFlags.Public) 
            ?? throw new ArgumentException(
                $"{property} does not exist as a public instance property on the type "
                + $"{inputType.FullName}."
                , nameof(property)
            );

        // Get the get method.
        var getMethod = propertyInfo.GetGetMethod() 
            ?? throw new ArgumentException(
                $"{property} on the type {inputType.FullName} does not have a getter."
                , nameof(property)
            );

        // If not public, throw.
        if (!getMethod.IsPublic)
            throw new ArgumentException(
                $"{property} on the type {inputType.FullName} does not have a public getter."
                , nameof(property)
        );

        // Return the property map.
        return new PropertyAccessorSqlBulkCopyMapperColumnMapping(
            ordinal
            , inputType
            , getMethod
        );
    }

    public static SqlBulkCopyMapperColumnMapping FromExpression<T>(
        int ordinal
        , Expression<Func<T, object?>> expression
    )
    {
        // If T is not fully public, then throw, since
        // there's nothing we can do.
        if (!typeof(T).IsFullyPublic())
            throw new InvalidOperationException(
                $"The type parameter passed to {nameof(FromExpression)} " 
                + $"({typeof(T).FullName}) must be public."
            );

        // Get the input type.
        var inputType = typeof(T);

        // This is a lambda, obviously, get the body.
        var body = expression.Body;

        // Look at the type of the body, if it is converting to object
        // then strip that.
        if (
            expression.Body.NodeType == ExpressionType.Convert
            && expression.ReturnType == typeof(object)
            && expression.Body is UnaryExpression u
        )
            // Set the body to the operand.
            body = u.Operand;

        // If this is a constant, then just use that.
        if (body is ConstantExpression ce)
            // Is the value null?  Return null mapping
            // otherwise return constant mapping.
            return ce.Value is null
                ? new NullSqlBulkCopyMapperColumnMapping(ordinal, inputType)
                : new ConstantSqlBulkCopyMapperColumnMapping(ordinal, inputType, ce.Value);

        // If this is a member access, then sniff the member.
        if (body is MemberExpression m)
        {
            // If it's a field, we can load the field, use that.
            if (
                m.Member is FieldInfo fi
                && fi.IsPublic
            )
                return new FieldAccessorSqlBulkCopyMapperColumnMapping(
                    ordinal
                    , inputType
                    , fi
                );

            // If this is a property and the getter is public
            // then use that.
            if (m.Member is PropertyInfo pi)
                // Return a property accessor.
                return FromInstanceProperty(ordinal, inputType, pi.Name);
        }

        // Throw.
        throw new ArgumentException(
            $"Could not construct an accessor from the {nameof(expression)} " 
            + "parameter."
            , nameof(expression)
        );
    }

    #endregion
}

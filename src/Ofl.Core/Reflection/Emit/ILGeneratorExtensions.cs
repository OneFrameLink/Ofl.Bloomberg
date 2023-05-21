using System.Reflection.Emit;

namespace Ofl.Core.Reflection.Emit;

public static class ILGeneratorExtensions
{
    public static ILGenerator PushThis(
        this ILGenerator generator
    ) => generator.PushArgument(0);

    public static ILGenerator PushLoadLocal(
        this ILGenerator generator
        , int index
    )
    {
        // Push the parameter on the stack.
        switch (index)
        {
            case 0:
                generator.Emit(OpCodes.Ldloc_0);
                break;
            case 1:
                generator.Emit(OpCodes.Ldloc_1);
                break;
            case 2:
                generator.Emit(OpCodes.Ldloc_2);
                break;
            case 3:
                generator.Emit(OpCodes.Ldloc_3);
                break;
            case > 3 and <= 255:
                generator.Emit(OpCodes.Ldloc_S, index);
                break;
            case > 255:
                generator.Emit(OpCodes.Ldloc, index);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                    , index
                    , $"The {nameof(index)} parameter must be a non-negative value (actual: {index})."
                );
        }

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushLoadLocalAddress(
        this ILGenerator generator
        , int index
    )
    {
        // Push the parameter on the stack.
        switch (index)
        {
            case >= 0 and <= 255:
                generator.Emit(OpCodes.Ldloca_S, index);
                break;
            case > 255:
                generator.Emit(OpCodes.Ldloca, index);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                    , index
                    , $"The {nameof(index)} parameter must be a non-negative value (actual: {index})."
                );
        }

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushStoreLocal(
        this ILGenerator generator
        , int index
    )
    {
        // Push the parameter on the stack.
        switch (index)
        {
            case 0:
                generator.Emit(OpCodes.Stloc_0);
                break;
            case 1:
                generator.Emit(OpCodes.Stloc_1);
                break;
            case 2:
                generator.Emit(OpCodes.Stloc_2);
                break;
            case 3:
                generator.Emit(OpCodes.Stloc_3);
                break;
            case > 3 and <= 255:
                generator.Emit(OpCodes.Stloc_S, index);
                break;
            case > 255:
                generator.Emit(OpCodes.Stloc, index);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                    , index
                    , $"The {nameof(index)} parameter must be a non-negative value (actual: {index})."
                );
        }

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushArgument(
        this ILGenerator generator
        , int index
    )
    {
        // Push the parameter on the stack.
        switch (index)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                    , index
                    , $"The {nameof(index)} parameter must be a non-negative value (actual: {index})."
                );
            case 0:
                generator.Emit(OpCodes.Ldarg_0);
                break;
            case 1:
                generator.Emit(OpCodes.Ldarg_1);
                break;
            case 2:
                generator.Emit(OpCodes.Ldarg_2);
                break;
            case 3:
                generator.Emit(OpCodes.Ldarg_3);
                break;

            // As per https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_s?view=net-7.0#:~:text=The%20ldarg.s%20instruction%20is%20an%20efficient%20encoding%20for%20loading%20arguments%20indexed%20from%204%20through%20255.
            // The call to Ldarg_S is more efficient for parameters 4 through 255
            case >= 4 and <= 255:
                generator.Emit(OpCodes.Ldarg_S, index);
                break;
            default:
                generator.Emit(OpCodes.Ldarg, index);
                break;
        }

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushArgumentAddress(
        this ILGenerator generator
        , int index
    )
    {
        // Push the parameter on the stack.
        switch (index)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                    , index
                    , $"The {nameof(index)} parameter must be a non-negative value (actual: {index})."
                );
            case >= 0 and <= 255:
                generator.Emit(OpCodes.Ldarga_S, index);
                break;
            default:
                generator.Emit(OpCodes.Ldarga, index);
                break;
        }

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushString(
        this ILGenerator generator
        , string value
    )
    {
        // Push the instruction.
        generator.Emit(OpCodes.Ldstr, value);

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushNull(
        this ILGenerator generator
    )
    {
        // Push the instruction.
        generator.Emit(OpCodes.Ldnull);

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushDouble(
        this ILGenerator generator
        , double value
    )
    {
        // Push the instruction.
        generator.Emit(OpCodes.Ldc_R8, value);

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushSingle(
        this ILGenerator generator
        , float value
    )
    {
        // Push the instruction.
        generator.Emit(OpCodes.Ldc_R4, value);

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushInt64(
        this ILGenerator generator
        , long value
    )
    {
        // Push the instruction.
        generator.Emit(OpCodes.Ldc_I8, value);

        // Return the generator.
        return generator;
    }

    public static ILGenerator PushInt32(
        this ILGenerator generator
        , int value
    )
    {
        // As per:
        // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_s?view=net-7.0#:~:text=ldc.i4.s%20is%20a%20more%20efficient%20encoding%20for%20pushing%20the%20integers%20from%20%2D128%20to%20127%20onto%20the%20evaluation
        // We can load up to 127 with the short form.
        // Switch.
        switch (value)
        {
            case -1:
                generator.Emit(OpCodes.Ldc_I4_M1);
                break;
            case 0:
                generator.Emit(OpCodes.Ldc_I4_0);
                break;
            case 1:
                generator.Emit(OpCodes.Ldc_I4_1);
                break;
            case 2:
                generator.Emit(OpCodes.Ldc_I4_2);
                break;
            case 3:
                generator.Emit(OpCodes.Ldc_I4_3);
                break;
            case 4:
                generator.Emit(OpCodes.Ldc_I4_4);
                break;
            case 5:
                generator.Emit(OpCodes.Ldc_I4_5);
                break;
            case 6:
                generator.Emit(OpCodes.Ldc_I4_6);
                break;
            case 7:
                generator.Emit(OpCodes.Ldc_I4_7);
                break;
            case 8:
                generator.Emit(OpCodes.Ldc_I4_8);
                break;
            case (>= -128 and < -1) or (> 8 and <= 127):
                generator.Emit(OpCodes.Ldc_I4_S, value);
                break;
            default:
                generator.Emit(OpCodes.Ldc_I4, value);
                break;
        }

        // Return the generator.
        return generator;
    }
}

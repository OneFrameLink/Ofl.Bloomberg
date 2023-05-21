namespace Ofl.Data.SqlClient;

internal static class TypeExtensions
{
    public static bool IsFullyPublic(this Type type)
    {
        // While the type is nested and public.
        while (type.IsNestedPublic)
            type = type.DeclaringType
                ?? throw new InvalidOperationException(
                    $"Reflection shows the type {type.FullName} as nexted and public "
                    + $"but the {nameof(Type.DeclaringType)} property returned null."
                );

        // Return whether or not the type is public.
        return type.IsPublic;
    }
}

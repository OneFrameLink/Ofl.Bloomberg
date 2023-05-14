namespace Ofl.Bloomberg.Fields.Tests.Files;

internal static class Resource
{
    #region Helpers

    private static Stream GetResourceStream(string resource) =>
        typeof(Resource)
            .Assembly
            .GetManifestResourceStream(typeof(Resource), resource)
            ?? throw new ArgumentException(
                $"Could not find a resource with the name {resource}"
            );

    #endregion

    #region Extensions.

    public static Stream GetFieldsCsvResourceStream() => GetResourceStream("fields.csv");

    #endregion
}

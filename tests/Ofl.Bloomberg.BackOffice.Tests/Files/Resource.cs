namespace Ofl.Bloomberg.BackOffice.Tests.Files;

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

    private static Stream GetFileStream(string resource) =>
        File.OpenRead(
            Path.Combine(
                "..\\..\\..\\..\\..\\files"
                , resource
            )
        );

    #endregion

    #region Extensions.

    public static Stream GetCreditRiskDifResourceStream() => GetResourceStream("credit_risk.dif");

    public static Stream GetCreditRiskDifFileStream() => GetFileStream("credit_risk.dif");

    public static Stream GetCreditRiskOutFileStream() => GetFileStream("credit_risk.out");

    public static Stream GetMsg1Parsing1600MuniOutResourceStream() => GetResourceStream("MSG1Parsing1600Muni.out");

    public static Stream GetMsg1Parsing1600MuniOutFileStream() => GetFileStream("MSG1Parsing1600Muni.out");

    #endregion
}

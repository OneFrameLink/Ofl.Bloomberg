using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static class BloombergBackOfficeDescriptiveDataFileRowExtensions
{
    #region Base return code

    // Based on:
    // https://data.bloomberg.com/docs/data-license/#bulk-file-types-DescriptiveDataFile
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldProcessBaseReturnCode(this BloombergBackOfficeDescriptiveDataFileRow row) =>
        (BloombergBackOfficeBaseReturnCode) row.ReturnCode == BloombergBackOfficeBaseReturnCode.ValidSecurity;

    #endregion

    #region Difference return code

    // Based on:
    // https://data.bloomberg.com/docs/data-license/#4-12-difference-dif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldProcessDifferenceReturnCode(
        this BloombergBackOfficeDescriptiveDataFileRow row
        , out bool removal
    )
    {
        // Default removal.
        removal = false;

        // Cast.
        var enumValue = (BloombergBackOfficeDifFileReturnCode) row.ReturnCode;

        // Common case, it's valid.
        if (enumValue == BloombergBackOfficeDifFileReturnCode.ValidSecurity)
            return true;

        // Is this a removal?
        removal = enumValue == BloombergBackOfficeDifFileReturnCode.Removal;

        // Bail.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldProcessDifferenceReturnCode(
        this BloombergBackOfficeDescriptiveDataFileRow row
    ) => row.ShouldProcessDifferenceReturnCode(out var _);

    #endregion
}

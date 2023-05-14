namespace Ofl.Bloomberg.Parsing;

// This is an amalgom of:
// https://data.bloomberg.com/docs/data-license/#4-80-return-values
// From the bulk data sets and
// https://data.bloomberg.com/docs/data-license/#9-10-1-2-data-not-returned
// For calls to getdata as they are overlapping/not different.
public enum ParsingDetail
{
    Ok = 0
    /// <summary>A blank field is returned, because requested field and security combination is not applicable</summary>
    , FieldSecurityCombinationNotApplicable
    /// <summary>Data is missing, because Bloomberg does not have the data</summary>
    , DataMissing
    /// <summary>Not downloadable, because user does not have permission to download the field</summary>
    , NotDownloadable
    /// <summary>Not subscribed, because user 1) is not entitled to download requested field and security combination, or 2) has hit the monthly limits on a test account</summary>
    , NotSubscribed
    /// <summary>Field unknown to Bloomberg</summary>
    , FieldUnknown
}

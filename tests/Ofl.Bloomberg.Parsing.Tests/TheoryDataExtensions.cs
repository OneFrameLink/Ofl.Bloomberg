using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;

namespace Ofl.Bloomberg.Parsing.Tests;

internal static class TheoryDataExtensions
{
    #region Extensions

    public static TheoryData<string, ParsingDetail> GetStandardInvalidInputParameters() => new() {
        // Empty.
        { "", ParsingDetail.FieldSecurityCombinationNotApplicable }

        // Whitespace
        , { " ", ParsingDetail.FieldSecurityCombinationNotApplicable }

        // Data missing
        , { "N.A.", ParsingDetail.DataMissing }

        // Not downloadable
        , { "N.D.", ParsingDetail.NotDownloadable }

        // Not subscribed
        , { "N.S.", ParsingDetail.NotSubscribed }

        // Field unknown
        , { "FLD UNKNOWN", ParsingDetail.FieldUnknown }
    };

    public static TheoryData<T1, T2> AddChained<T1, T2>(
        this TheoryData<T1, T2> theoryData
        , T1 parameter1
        , T2 parameter2
    )
    {
        // Add.
        theoryData.Add(parameter1, parameter2);

        // Return the theory data.
        return theoryData;
    }

    public static TheoryData<string, ParsingDetail> AddInvalidInput(
        this TheoryData<string, ParsingDetail> theoryData
        , string input
        , ParsingDetail parsingDetail = ParsingDetail.Ok
    ) => theoryData.AddChained(input, parsingDetail);

    #endregion
}

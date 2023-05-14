using System.Runtime.CompilerServices;
using Ofl.Bloomberg.Fields.DataTransferObjects;
using Sylvan.Data.Csv;

namespace Ofl.Bloomberg.Fields;

public static class StreamExtensions
{
    #region Helpers

    private static FieldDataTransferObject Map(this CsvDataReader reader) => new() {
        FieldId = reader.GetString(0)
        , FieldMnemonic = reader.GetString(1)
        , Description = reader.GetString(2)
        , DataLicenseCategory = reader.GetString(3)
        , Category = reader.GetString(4)
        , Definition = reader.GetString(5)
        , Comdty = reader.GetString(6)
        , Equity = reader.GetString(7)
        , Muni = reader.GetString(8)
        , Pfd = reader.GetString(9)
        , MMkt = reader.GetString(10)
        , Govt = reader.GetString(11)
        , Corp = reader.GetString(12)
        , Index = reader.GetString(13)
        , Curncy = reader.GetString(14)
        , Mtge = reader.GetString(15)
        , StandardWidth = reader.GetString(16)
        , StandardDecimalPlaces = reader.GetString(17)
        , FieldType = reader.GetString(18)
        , BackOffice = reader.GetString(19)
        , ExtendedBackOffice = reader.GetString(20)
        , ProductionDate = reader.GetString(21)
        , CurrentMaximumWidth = reader.GetString(22)
        , Bval = reader.GetString(23)
        , BvalBlocked = reader.GetString(24)
        , GetFundamentals = reader.GetString(25)
        , GetHistory = reader.GetString(26)
        , GetCompany = reader.GetString(27)
        , OldMnemonic = reader.GetString(28)
        , DataLicenseCategory2 = reader.GetString(29)
        , PsboOpt = reader.GetString(30)
        , DsBvalMetered = reader.GetString(31)
        , DlBoOptFundamentals = reader.GetString(32)
        , DlBoOptBdvd = reader.GetString(33)
        , DlBoOptBest = reader.GetString(34)
        , DlBoOptCreditRisk = reader.GetString(35)
        , DlBoOptCapStruct = reader.GetString(36)
        , DlBoOptCreditRiskGetCompany = reader.GetString(37)
        , DlBoOptCapStructGetCompany = reader.GetString(38)
        , SapiOms = reader.GetString(39)
        , DlBoOptRegCompliance = reader.GetString(40)
        , DlBoOptIssuerRatings = reader.GetString(41)
        , XsdType = reader.GetString(42)
        , XsdMinInclusive = reader.GetString(43)
        , XsdMaxInclusive = reader.GetString(44)
        , XsdMinExclusive = reader.GetString(45)
        , XsdMaxExclusive = reader.GetString(46)
        , XsdFractionDigits = reader.GetString(47)
        , XsdMinLength = reader.GetString(48)
        , XsdMaxLength = reader.GetString(49)
        , XsdLength = reader.GetString(50)
        , XsdPattern = reader.GetString(51)
        , RdfLangRange = reader.GetString(52)
        , NamedPropertyIri = reader.GetString(53)
        , SuperPropertyIri = reader.GetString(54)
        , IsAbstract = reader.GetString(55)
        , CleanName = reader.GetString(56)
        , NPort = reader.GetString(57)
    };

    internal static async IAsyncEnumerable<FieldDataTransferObject> EnumerateFieldDtos(
        this Stream stream
        ,[EnumeratorCancellation] 
        CancellationToken cancellationToken = default
    )
    {
        // Create a stream reader.
        using var sr = new StreamReader(stream);

        // Create the reader.
        using var csv = await CsvDataReader.CreateAsync(
            sr
            , new CsvDataReaderOptions {
                Schema = CsvSchema.Nullable
                , HasHeaders = true
                , Delimiter = ','
                // Obtained through trial and error.
                , BufferSize = 1024 * 64
            }
        );

        // Cycle while there are fields.
        while (await csv.ReadAsync(cancellationToken).ConfigureAwait(false))
            yield return csv.Map();
    }

    #endregion

    #region Extensions

    public static IAsyncEnumerable<Field> EnumerateFields(
        this Stream stream
        , CancellationToken cancellationToken = default
    ) => stream
        .EnumerateFieldDtos(cancellationToken)
        .Select(FieldDataTransferObjectToFieldMapper.ToField);

    #endregion
}

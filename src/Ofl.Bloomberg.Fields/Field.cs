namespace Ofl.Bloomberg.Fields;

// NOTE: DO NOT CHANGE THE ORDER OF THE PROPERTIES.
// THE ORDER IS THE SAME AS THE FIELD ORDER IN FIELDS.CSV
public record Field : IEquatable<Field>
{
    #region Fields

    public required string FieldId { get; init; }

    public required string FieldMnemonic { get; init; }

    public required string Description { get; init; }

    public required string? DataLicenseCategory { get; init; }

    public required string Category { get; init; }

    public required string Definition { get; init; }

    public required bool? Comdty { get; init; }

    public required bool? Equity { get; init; }

    public required bool? Muni { get; init; }

    public required bool? Pfd { get; init; }

    public required bool? MMkt { get; init; }

    public required bool? Govt { get; init; }

    public required bool? Corp { get; init; }

    public required bool? Index { get; init; }

    public required bool? Curncy { get; init; }

    public required bool? Mtge { get; init; }

    public required int StandardWidth { get; init; }

    public required int StandardDecimalPlaces { get; init; }

    public required string FieldType { get; init; }

    public required bool? BackOffice { get; init; }

    public required bool? ExtendedBackOffice { get; init; }

    public required DateOnly ProductionDate { get; init; }

    public required int CurrentMaximumWidth { get; init; }

    public required bool? Bval { get; init; }

    public required bool? BvalBlocked { get; init; }

    public required bool? GetFundamentals { get; init; }

    public required bool? GetHistory { get; init; }

    public required bool? GetCompany { get; init; }

    public required string? OldMnemonic { get; init; }

    public required string? DataLicenseCategory2 { get; init; }

    public required bool PsboOpt { get; init; }

    public required bool? DsBvalMetered { get; init; }

    public required bool DlBoOptFundamentals { get; init; }

    public required bool DlBoOptBdvd { get; init; }

    public required bool DlBoOptBest { get; init; }

    public required bool DlBoOptCreditRisk { get; init; }

    public required bool DlBoOptCapStruct { get; init; }

    public required bool DlBoOptCreditRiskGetCompany { get; init; }

    public required bool DlBoOptCapStructGetCompany { get; init; }

    public required bool SapiOms { get; init; }

    public required bool DlBoOptRegCompliance { get; init; }

    public required bool DlBoOptIssuerRatings { get; init; }

    public required string XsdType { get; init; }

    public required int? XsdMinInclusive { get; init; }

    public required long? XsdMaxInclusive { get; init; }

    public required decimal? XsdMinExclusive { get; init; }

    public required long? XsdMaxExclusive { get; init; }

    public required int? XsdFractionDigits { get; init; }

    public required int? XsdMinLength { get; init; }

    public required int? XsdMaxLength { get; init; }

    public required int? XsdLength { get; init; }

    public required string? XsdPattern { get; init; }

    public required string? RdfLangRange { get; init; }

    public required Uri? NamedPropertyIri { get; init; }

    public required Uri? SuperPropertyIri { get; init; }

    public required bool IsAbstract { get; init; }

    public required string CleanName { get; init; }

    public required string? NPort { get; init; }

    #endregion

    #region Equality

    public virtual bool Equals(Field? that) => that?.FieldId == FieldId;

    public override int GetHashCode() => FieldId.GetHashCode();

    #endregion
}
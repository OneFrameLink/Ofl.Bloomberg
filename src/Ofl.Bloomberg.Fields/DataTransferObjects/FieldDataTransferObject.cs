namespace Ofl.Bloomberg.Fields.DataTransferObjects;

// NOTE: DO NOT CHANGE THE ORDER OF THE PROPERTIES.
// THE ORDER IS THE SAME AS THE FIELD ORDER IN FIELDS.CSV
internal class FieldDataTransferObject
{
    public required string FieldId { get; init; }

    public required string FieldMnemonic { get; init; }

    public required string Description { get; init; }

    public required string? DataLicenseCategory { get; init; }
    
    public required string Category { get; init; }
    
    public required string Definition { get; init; }
    
    public required string? Comdty { get; init; }
    
    public required string? Equity { get; init; }
    
    public required string? Muni { get; init; }
    
    public required string? Pfd { get; init; }
    
    public required string? MMkt { get; init; }
    
    public required string? Govt { get; init; }
    
    public required string? Corp { get; init; }
    
    public required string? Index { get; init; }
    
    public required string? Curncy { get; init; }
    
    public required string? Mtge { get; init; }
    
    public required string StandardWidth { get; init; }
    
    public required string StandardDecimalPlaces { get; init; }
    
    public required string FieldType { get; init; }
    
    public required string? BackOffice { get; init; }
    
    public required string? ExtendedBackOffice { get; init; }
    
    public required string ProductionDate { get; init; }
    
    public required string CurrentMaximumWidth { get; init; }
    
    public required string? Bval { get; init; }
    
    public required string? BvalBlocked { get; init; }
    
    public required string? GetFundamentals { get; init; }
    
    public required string? GetHistory { get; init; }
    
    public required string? GetCompany { get; init; }
    
    public required string? OldMnemonic { get; init; }
    
    public required string? DataLicenseCategory2 { get; init; }
    
    public required string PsboOpt { get; init; }
    
    public required string? DsBvalMetered { get; init; }
    
    public required string DlBoOptFundamentals { get; init; }
    
    public required string DlBoOptBdvd { get; init; }
    
    public required string DlBoOptBest { get; init; }
    
    public required string DlBoOptCreditRisk { get; init; }
    
    public required string DlBoOptCapStruct { get; init; }
    
    public required string DlBoOptCreditRiskGetCompany { get; init; }
    
    public required string DlBoOptCapStructGetCompany { get; init; }
    
    public required string SapiOms { get; init; }
    
    public required string DlBoOptRegCompliance { get; init; }
    
    public required string DlBoOptIssuerRatings { get; init; }
    
    public required string XsdType { get; init; }
    
    public required string? XsdMinInclusive { get; init; }
    
    public required string? XsdMaxInclusive { get; init; }
    
    public required string? XsdMinExclusive { get; init; }
    
    public required string? XsdMaxExclusive { get; init; }
    
    public required string? XsdFractionDigits { get; init; }
    
    public required string? XsdMinLength { get; init; }
    
    public required string? XsdMaxLength { get; init; }
    
    public required string? XsdLength { get; init; }
    
    public required string? XsdPattern { get; init; }
    
    public required string? RdfLangRange { get; init; }
    
    public required string? NamedPropertyIri { get; init; }
    
    public required string? SuperPropertyIri { get; init; }
    
    public required string IsAbstract { get; init; }
    
    public required string CleanName { get; init; }
 
    public required string? NPort { get; init; }
}
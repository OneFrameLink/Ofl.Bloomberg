namespace Ofl.Bloomberg.Fields;

public static class FieldExtensions
{
    // NOTE: This was done using an analysis of the fields.csv file obtained from Bloomberg
    // on April 12, 2023.
    // NOTE: May be obsoleted by:
    // https://data.bloomberg.com/docs/data-license/#per-security-DataTypes
    // And takin into account by parsing.
    public static Type GetClrType(this Field field)
    {
        // This has two fields which a field type of character and expressive of boolean
        // values, otherwise, everything else is Boolean.
        if (field.XsdType == "xsd:boolean") return typeof(bool);

        // Small subsection are actual times.
        if (field.XsdType == "xsd:NCName")
            // If this is Time, then return time, otherwise, return string.
            return field.FieldType == "Time"
                ? typeof(TimeOnly)
                : typeof(string);

        // Everything but a small subset is strings.
        if (field.XsdType == "xsd:normalizedString")
            // If the field type is Integer, return int, otherwise, return
            // string.
            return field.FieldType == "Integer"
                ? typeof(int)
                : typeof(string);

        // All of these are Time
        if (field.XsdType == "xsd:time") return typeof(TimeOnly);

        // Integers are by and large integer types.
        // There are some that are real, which will be typed as decimal.
        // There are a handful that are typed as character, which are going to be
        // typed as integer as well.
        if (field.XsdType == "xsd:integer")
            return field.FieldType == "Real"
                ? typeof(decimal)
                : typeof(int);

        // Decimals are by and large decimal types.
        if (field.XsdType == "xsd:decimal")
            // Switch on the field type.
            return field.FieldType switch {
                // Only one field
                "Date" => typeof(DateOnly)
                // Covers Price, Real, Integer, Character, Integer/Real
                , _ => typeof(decimal)
            };

        // Date are mostly date only.
        if (field.XsdType == "xsd:date")
            // If the field is equal to RELATIONSHIP_YEAR (DZ409), then this is an integer.
            // This is the one outlier in the character space.
            return field.FieldId == "DZ409"
                ? typeof(int)
                : typeof(DateOnly);


        // xsd:token requires looking at FieldType.
        if (field.XsdType == "xsd:token")
            // Switch on field type.
            return field.FieldType switch
            {
                // Boolean (one field)
                "Boolean" => typeof(bool)
                // One field.
                , "Integer" => typeof(int)
                // Will require special provisioning when parsing to check for these formats.
                // Since the date portion will be set to 1.
                , "Month/Year" => typeof(DateOnly)
                , "Time" => typeof(TimeOnly)
                // Default to string.
                // Covers Bulk format, character, Date or Time (since we can't tell which it is)
                // Long Character (although some of these look suspicous, see "%_SHARES_SOLD_BY_ADVISERS")
                , _ => typeof(string)
            };

        // Default to string.
        // Covers unknowns, but also:
        // xsd:anySimpleType
        // xsd:anyURI
        // xsd:string
        // xsd:NMTOKEN
        // xsd:dateTime ➡ string, as they are either dates OR times.
        // xsd:gYear ➡ Omits six fields. Could possibly map to an int, but the spec
        // https://www.w3.org/TR/xmlschema11-2/#gYear
        // allows for fragments of time zones
        // to be included, so default to string for now.
        return typeof(string);
    }
}

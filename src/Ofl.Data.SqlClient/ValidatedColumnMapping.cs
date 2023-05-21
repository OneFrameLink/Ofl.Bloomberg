using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient;

internal record struct ValidatedColumnMapping(
    SqlBulkCopyMapperColumnMapping Mapping
    , FieldBuilder? MapFieldBuilder
    , ValidatedColumnBoxedValueMapping? Boxing
    , Label Label
);

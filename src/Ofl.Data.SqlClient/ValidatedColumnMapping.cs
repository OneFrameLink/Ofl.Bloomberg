using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient;

internal record struct ValidatedColumnMapping(
    SqlBulkCopyMapperColumnMapping Mapping
    , Type ReturnType
    , FieldBuilder MapFieldBuilder
    , MethodInfo MapFieldBuilderGetMethod
    , ValidatedColumnBoxedValueMapping? Boxing
);

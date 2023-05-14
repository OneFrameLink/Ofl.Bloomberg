using System.Reflection;
using System.Reflection.Emit;

namespace Ofl.Data.SqlClient;

internal record struct ValidatedColumnBoxedValueMapping(
    Type Type
    , FieldBuilder ReusableBoxFieldBuilder
    , bool Nullable
    , MethodInfo ReusableBoxGetBoxMethodInfo
);

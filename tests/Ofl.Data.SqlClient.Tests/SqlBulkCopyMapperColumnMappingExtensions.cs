using System.ComponentModel;

namespace Ofl.Data.SqlClient.Tests;

public partial class SqlBulkCopyMapperColumnMappingExtensions
{

    #region Helpers

    #region Helpers

    public static int GetReferenceTypeInputIntValue(
        in ReferenceTypeInput x
    ) => x.Int32Value;

    #endregion

    public static IReadOnlyCollection<SqlBulkCopyMapperColumnMapping> CreateInputMappings() =>
        new SqlBulkCopyMapperColumnMapping[]
        {
            // Static mapping.
            SqlBulkCopyMapperColumnMapping.FromDelegate<ReferenceTypeInput, int>(
                0
                , GetReferenceTypeInputIntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                1
                , (in ReferenceTypeInput x) => x.NullableIntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                2
                ,(in ReferenceTypeInput x) => x.StringValue
            )
        }
        .AsReadOnly();

    private static (SqlBulkCopyMapperColumnMapping mapping, int ordinal, object? expected) CreateMapperAndExpected<TInput>(
        bool mapperIsDelegate
        , ResultType resultType
    )
    {
        // An exception, in case.
        var exception = new InvalidEnumArgumentException(
            nameof(resultType)
            , (int) resultType
            , typeof(ResultType)
        );

        // Switch on the mapper being a delegate or not.
        if (mapperIsDelegate)
        {
            // Helper to gte the results for delegate.
            static (SqlBulkCopyMapperColumnMapping mapping, int ordinal, object? expected) FromDelegate<TProperty>(
                TProperty value
            ) =>
                (
                    SqlBulkCopyMapperColumnMapping.FromDelegate(
                        0
                        , (in TInput _) => value
                    )
                    , 0
                    , value
                );

            // Switch on the result type.
            return resultType switch {
                ResultType.ReferenceTypeNull => FromDelegate((string?) null)
                , ResultType.ReferenceTypeNotNull => FromDelegate("Hello")
                , ResultType.NonNullValueType => FromDelegate(10)
                , ResultType.NullableValueTypeNull => FromDelegate((int?) null)
                , ResultType.NullableValueTypeNotNull => FromDelegate((int?) 10)
                , _ => throw exception
            };
        }

        // Helper to return a duck typed mapper.
        static (SqlBulkCopyMapperColumnMapping mapping, int ordinal, object? expected) FromDuckType<TProperty>(
            TProperty value
        ) =>
            (
                SqlBulkCopyMapperColumnMapping.FromDuckTypedObjectWithMapMethod<TInput>(
                    0
                    , new DuckTypedMapper<TInput, TProperty>(value)
                )
                , 0
                , value
            );

        // Switch on the result type.
        return resultType switch {
            ResultType.ReferenceTypeNull => FromDuckType((string?) null)
            , ResultType.ReferenceTypeNotNull => FromDuckType("Hello")
            , ResultType.NonNullValueType => FromDuckType(10)
            , ResultType.NullableValueTypeNull => FromDuckType((int?) null)
            , ResultType.NullableValueTypeNotNull => FromDuckType((int?) 10)
            , _ => throw exception
        };
    }

    private static void Assert_Test_Reference_Type_Mapping_Succeeds<T>(
        bool mapperIsDelegate
        , ResultType resultType
    ) where T : new()
    {
        // Create the mapper and the expected.
        var (mapping, ordinal, expected) = CreateMapperAndExpected<T>(
            mapperIsDelegate
            , resultType
        );

        // Now create the mapper.
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<T>(
                new[] { mapping }
            );

        // Assert the field count.
        Assert.Equal(1, mapper.FieldCount);

        // The input.
        var input = new T();

        // Get the actual value.
        var actual = mapper.Map(in input, ordinal);

        // They are equal.
        Assert.Equal(expected, actual);
    }

    // Creates stepped mappings.
    private static IEnumerable<int> CreateSteppedMappings(
        int columns
        , int offset = 0
        , int step = 1
    )
    {
        // The last value.
        var last = offset;

        // Cycle through the count, increment
        // with offset.
        for (int i = 0; i < columns; ++i)
        {
            // Yield the last.
            yield return last;

            // Increment by step.
            last += step;
        }
    }

    #endregion

    #region Tests

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_ReferenceType()
    {
        // Create the mapper for the nullable int value.
        var nullableIntValueMapper =
            new InstanceValueMapper<ReferenceTypeInput, int?>(
                (in ReferenceTypeInput input) => input.NullableIntValue
            );

        // The date time mapper.
        var dateTimeMapper =
            new InstanceValueMapper<ReferenceTypeInput, DateTime?>(
                (in ReferenceTypeInput input) => input.DateTimeValue
            );

        // Create the mappings.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            // Static delegate mapping.
            SqlBulkCopyMapperColumnMapping.FromDelegate<ReferenceTypeInput, int>(
                0
                , GetReferenceTypeInputIntValue
            )
            // Delegate on public instance method on public type.
            , SqlBulkCopyMapperColumnMapping.FromDelegate<ReferenceTypeInput, int?>(
                1
                , nullableIntValueMapper.Map
            )
            // Private delegate
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                2
                , (in ReferenceTypeInput x) => x.StringValue
            )
            // Duck type.
            , SqlBulkCopyMapperColumnMapping.FromDuckTypedObjectWithMapMethod<ReferenceTypeInput>(
                3
                , dateTimeMapper
            )
            // Expression on publicly available item
            , SqlBulkCopyMapperColumnMapping.FromExpression(
                4
                , (ReferenceTypeInput x) => x.Int64Value
            )
            // Expression on constant.
            , SqlBulkCopyMapperColumnMapping.FromExpression(
                5
                , (ReferenceTypeInput x) => 42
            )
            // Null.
            , SqlBulkCopyMapperColumnMapping.FromExpression(
                6
                , (ReferenceTypeInput x) => null
            )
            // Field
            , SqlBulkCopyMapperColumnMapping.FromExpression(
                7
                , (ReferenceTypeInput x) => x.FieldInt32Value
            )
        }
        .AsReadOnly();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings);

        // Create a value.
        var input = new ReferenceTypeInput {
            DateTimeValue = DateTime.Now
            , Int32Value = 1
            , NullableIntValue = 2
            , StringValue = "test"
            , Int64Value = long.MaxValue
            , FieldInt32Value = int.MaxValue
        };

        // Check.
        Assert.Equal(input.Int32Value, mapper.Map(in input, 0));
        Assert.Equal(input.NullableIntValue, mapper.Map(in input, 1));
        Assert.Equal(input.StringValue, mapper.Map(in input, 2));
        Assert.Equal(input.DateTimeValue, mapper.Map(in input, 3));
        Assert.Equal(input.Int64Value, mapper.Map(in input, 4));
        Assert.Equal(42, mapper.Map(in input, 5));
        Assert.Null(mapper.Map(in input, 6));
        Assert.Equal(input.FieldInt32Value, mapper.Map(in input, 7));
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_FieldCount()
    {
        // Create the mappings.
        var mappings = CreateInputMappings();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings);

        // Field count is three.
        Assert.Equal(3, mapper.FieldCount);
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_1000_Columns_With_1000_Offset_And_Two_Buckets_Of_500_Separated_By_2000_Succeeds()
    {
        // The groups
        var firstIndices = CreateSteppedMappings(
            500
            , 1000
        );
        var secondIndices = CreateSteppedMappings(
            500
            , 3500
        ); 

        // Get the mappings.
        var mappings = firstIndices
            .Concat(secondIndices)
            .Select((n, i) => SqlBulkCopyMapperColumnMapping.FromDelegate(
                n
                , (in object _) => n
            ))
            .ToList()
            .AsReadOnly();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<object>(mappings);

        // Cycle through the mappings and access.
        foreach (var m in mappings)
        {
            // Get the actual value.
            var actual = mapper.Map(null!, m.Ordinal);

            // Assert.
            Assert.Equal(m.Ordinal, actual);
        }
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_1000_Columns_With_1000_Offset_And_Step_Of_1000_Succeeds()
    {
        // Create the mappings.
        var mappings = CreateSteppedMappings(
            1000
            , 1000
            , 1000
        )
        .Select((n, i) => SqlBulkCopyMapperColumnMapping.FromDelegate(
            n
            , (in object _) => n
        ))
        .ToList()
        .AsReadOnly();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<object>(mappings);

        // Cycle through the mappings and access.
        foreach (var m in mappings)
        {
            // Get the actual value.
            var actual = mapper.Map(null!, m.Ordinal);

            // Assert.
            Assert.Equal(m.Ordinal, actual);
        }
    }

    public static IEnumerable<object[]> Test_Mapping_Succeeds_Parameters()
    {
        // Cycle through true false for mapper is delegate.
        foreach (bool mapperIsDelegate in new[] { true, false })
        foreach (ResultType resultType in Enum.GetValues(typeof(ResultType)))
            yield return new object[] { mapperIsDelegate, resultType };
    }

    [Theory]
    [MemberData(nameof(Test_Mapping_Succeeds_Parameters))]
    public void Test_Reference_Type_Mapping_Succeeds(
        bool mapperIsDelegate
        , ResultType resultType
    ) => Assert_Test_Reference_Type_Mapping_Succeeds<object>(mapperIsDelegate, resultType);

    [Theory]
    [MemberData(nameof(Test_Mapping_Succeeds_Parameters))]
    public void Test_Non_Nullable_Value_Type_Mapping_Succeeds(
        bool mapperIsDelegate
        , ResultType resultType
    ) => Assert_Test_Reference_Type_Mapping_Succeeds<int>(mapperIsDelegate, resultType);

    [Theory]
    [MemberData(nameof(Test_Mapping_Succeeds_Parameters))]
    public void Test_Nullable_Value_Type_Mapping_Succeeds(
        bool mapperIsDelegate
        , ResultType resultType
    ) => Assert_Test_Reference_Type_Mapping_Succeeds<int?>(mapperIsDelegate, resultType);

    #endregion
}
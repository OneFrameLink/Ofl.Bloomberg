using System.ComponentModel;

namespace Ofl.Data.SqlClient.Tests;

public partial class SqlBulkCopyMapperColumnMappingExtensions
{

    #region Helpers

    public static IReadOnlyCollection<SqlBulkCopyMapperColumnMapping> CreateInputMappings() =>
        new SqlBulkCopyMapperColumnMapping[]
        {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.IntValue), 0)
                , (in ReferenceTypeInput x) => x.IntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.NullableIntValue), 1)
                , (in ReferenceTypeInput x) => x.NullableIntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.StringValue), 2)
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
                        new TestDbColumn("dummy", 0)
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
                SqlBulkCopyMapperColumnMapping.FromDuckTypedObjectWithMapMethod(
                    new TestDbColumn("dummy", 0)
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
                new TestDbColumn($"c{n}", n)
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
            var actual = mapper.Map(null!, m.Column.ColumnOrdinal!.Value);

            // Assert.
            Assert.Equal(m.Column.ColumnOrdinal!.Value, actual);
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
            new TestDbColumn($"c{n}", n)
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
            var actual = mapper.Map(null!, m.Column.ColumnOrdinal!.Value);

            // Assert.
            Assert.Equal(m.Column.ColumnOrdinal!.Value, actual);
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
using Ofl.Core;
using Ofl.Core.Reflection.Emit;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Ofl.Data.SqlClient;

public static class SqlBulkCopyMapperColumnMappingExtensions
{
    #region Helpers

    private static TypeBuilder CreateTypeBuilder<T>()
    {
        // Get the current guid discriminator.
        var discriminator = Guid.NewGuid().ToString("N");

        // Get the name.
        var name = $"{typeof(SqlBulkCopyMapperColumnMappingExtensions).FullName}.Dynamic"
            + $".SqlBulkCopyMapper._{discriminator}";

        // Create the assembly name.
        var assemblyName = new AssemblyName(name);

        // Create the assembly builder.  Collect when done.
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.RunAndCollect
        );

        // Create a module, same ass assembly name but with ".dll"
        var moduleBuilder = assemblyBuilder
            .DefineDynamicModule($"{name}.dll");

        // Create the type.
        var typeBuilder = moduleBuilder.DefineType(
            $"SqlBulkCopyMapper_{discriminator}"
            // No need to allow it to be extended.
            , TypeAttributes.Public 
            | TypeAttributes.Sealed
            , typeof(SqlBulkCopyRowMapperBase<T>)
         );

        // Return the type builder.
        return typeBuilder;
    }

    private static MethodInfo ValidateRowValueMapper<T>(
        this SqlBulkCopyMapperColumnMapping columnMapping
    )
    {
        // Get the input type.
        var inputType = typeof(T).MakeByRefType();

        // Get the mapper type.
        var valueMapperType = columnMapping
            .RowValueAccessor
            .GetType();

        // If not a class or not public, throw.
        if (!valueMapperType.IsClass || !valueMapperType.IsPublic)
            throw new ArgumentException(
                $"The mapping for column with ordinal {columnMapping.Ordinal} must be "
                + "a public reference type."
                , nameof(columnMapping)
            );

        // Is this a delegate? If so, then just access directly.
        MethodInfo mapMethodInfo = columnMapping.ValidateDuckTypedMapping(            
            inputType
            , valueMapperType
            , valueMapperType.IsAssignableTo(typeof(Delegate))
        );

        // Return the method info.
        return mapMethodInfo;
    }

    private static MethodInfo ValidateDuckTypedMapping(
        this SqlBulkCopyMapperColumnMapping columnMapping
        , Type inputType
        , Type valueMapperType
        , bool isDelegate
    )
    {
        // The map method name.  If it is a delegate then we
        // want the Invoke method.
        var mapMethodName = isDelegate
            ? "Invoke"
            : nameof(ISqlBulkCopyRowValueMapper<object, object>.Map);

        // There must be a Map method that takes an in input of
        // type T and return any type.
        // Cycle through, as we want to be as open as possible.
        var mapMethodInfos = valueMapperType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == mapMethodName);

        // The singular map method info.
        MethodInfo mapMethodInfo = default!;

        // Cycle.
        foreach (var localMapMethodInfo in mapMethodInfos)
        {
            // Get the parameters.
            var parameters = localMapMethodInfo.GetParameters();

            // Is there one?  If not, continue.
            if (parameters.Length != 1) continue;

            // Get the parameter.
            var parameter = parameters.Single();

            // Is the first parameter assignable from the input type?
            if (!parameter.ParameterType.IsAssignableFrom(inputType))
                continue;

            // Is it an in parameter?  If not, continue.
            // TODO: consider if ref is ok here as well.
            if (!parameter.IsIn) continue;

            // Assign the method info.
            // If already assigned, throw.
            if (mapMethodInfo is not null)
                throw new InvalidOperationException(
                    $"The {nameof(SqlBulkCopyMapperColumnMapping.RowValueAccessor)} of type {valueMapperType.FullName} "
                    + $"assigned to the column with ordinal {columnMapping.Ordinal} "
                    + $"has multiple public {mapMethodName} methods that take "
                    + $"an input of type {inputType.FullName} as an in parameter."
                );

            // Assign.
            mapMethodInfo = localMapMethodInfo;
        }

        // If there are no methods, throw.
        if (mapMethodInfo is null)
            throw new InvalidOperationException(
                $"The {nameof(SqlBulkCopyMapperColumnMapping.RowValueAccessor)} of type {valueMapperType.FullName} "
                + $"assigned to the column with ordinal {columnMapping.Ordinal} "
                + $"must implement a public {mapMethodName} method that "
                + $"takes an input of type {inputType.FullName} as an in parameter and returns any type."
            );

        // Return the map method info.
        return mapMethodInfo;
    }

    private static ValidatedColumnMapping ValidateColumnMapping<T>(
        this SqlBulkCopyMapperColumnMapping columnMapping
        , IDictionary<int, SqlBulkCopyMapperColumnMapping> existingColumnOrdinals
    )
    {
        // The ordinal must not be negative.
        if (columnMapping.Ordinal < 0)
            throw new InvalidOperationException(
                $"The column with ordinal {columnMapping.Ordinal} cannot have a negative "
                + $"{nameof(columnMapping.Ordinal)}."
            );

        // Check the column ordinal.  If set, throw.
        if (existingColumnOrdinals.TryGetValue(
            columnMapping.Ordinal
            , out _
        ))
            throw new ArgumentException(
                $"The ordinal {columnMapping.Ordinal} is mapped multiple times."
                , nameof(columnMapping)
            );

        // Add.
        existingColumnOrdinals.Add(columnMapping.Ordinal, columnMapping);

        // Get the map method.
        var mapMethod = columnMapping.ValidateRowValueMapper<T>();

        // Return the validated column mapping.
        return new(
            columnMapping
            , mapMethod.ReturnType
            , default!
            , mapMethod
            , default!
            , default
        );
    }

    private static List<ValidatedColumnMapping> ValidateColumnMappings<T>(
        this IReadOnlyCollection<SqlBulkCopyMapperColumnMapping> columnMappings
    )
    {
        // There must be at least one element.
        if (!columnMappings.Any())
            throw new ArgumentException(
                $"The {nameof(columnMappings)} parameter must have at least one item in it."
                , nameof(columnMappings)
            );

        // A dictionary to hold existing mappings.
        var existingOrdinals = new Dictionary<int, SqlBulkCopyMapperColumnMapping>();

        // Map and return.
        return columnMappings
            .Select(m => m.ValidateColumnMapping<T>(existingOrdinals))
            .OrderBy(m => m.Mapping.Ordinal)
            .ToList();
    }

    private static void CreateFields(
        this TypeBuilder typeBuilder
        , IList<ValidatedColumnMapping> columnMappings
    )
    {
        // Cycle through the fields...
        for (int i = 0; i < columnMappings.Count; ++i)
        {
            // Get the mapping.
            var mapping = columnMappings[i];

            // Create a field builder and set.
            mapping = mapping with {
                MapFieldBuilder = typeBuilder
                    .DefineField(
                        $"_mapper{mapping.Mapping.Ordinal}"
                        , mapping.Mapping.RowValueAccessor.GetType()
                        , FieldAttributes.Private | FieldAttributes.InitOnly
                    )
            };

            // If this is not a reference type, create a reusable boxed
            // instance of the field.
            if (mapping.ReturnType.IsValueType)
            {
                // The boxed type.
                var boxedType = mapping.ReturnType;

                // Is this nullable?
                var nullable = boxedType.IsGenericType
                    && boxedType.GetGenericTypeDefinition() == typeof(Nullable<>);

                // If this is generic and nullable, then we need to get the
                // underlying value type.
                if (nullable)
                    boxedType = boxedType.GetGenericArguments().Single();

                // The type of the reusable box.
                var reusableBoxType = typeof(ReusableBox<>).MakeGenericType(boxedType);

                // Create the field builder.
                var fieldBuilder = typeBuilder.DefineField(
                    $"_boxed{mapping.Mapping.Ordinal}"
                    , reusableBoxType
                    , FieldAttributes.Private | FieldAttributes.InitOnly
                );

                // Get the method info for get box.
                const string getBoxMethodName = nameof(ReusableBox<int>.GetBox);

                // Get the method.
                var getBoxMethod = fieldBuilder
                    .FieldType
                    .GetMethod(
                        getBoxMethodName
                        , BindingFlags.Public | BindingFlags.Instance
                    )
                    ?? throw new InvalidOperationException(
                        $"Could not find the public method {getBoxMethodName} exposed on the type "
                        + $"{fieldBuilder.FieldType.FullName} for the column mapping with "
                        + $"ordinal {mapping.Mapping.Ordinal} and name "
                        + $"{mapping.Mapping.Ordinal}."
                    );

                // Create the boxing information.
                ValidatedColumnBoxedValueMapping boxing = new(
                    boxedType
                    , fieldBuilder
                    , nullable
                    , getBoxMethod
                );

                // Create a boxed field.
                mapping = mapping with {
                    Boxing = boxing
                };
            }

            // Set.
            columnMappings[i] = mapping;
        }
    }

    private static object[] CreateConstructor<T>(
        this TypeBuilder typeBuilder
        , IReadOnlyCollection<ValidatedColumnMapping> columnMappings
    )
    {
        // The base type.
        var baseType = typeof(SqlBulkCopyRowMapperBase<>)
            .MakeGenericType(typeof(T));

        // Get the boxable count.
        var boxables = columnMappings
            .Where(m => m.Boxing is not null)
            .Count();

        // Define the constructor that takes all the delegates.
        ConstructorBuilder constructor = typeBuilder.DefineConstructor(
            MethodAttributes.Public
            , CallingConventions.Standard
            , columnMappings.Select(m => m.Mapping.RowValueAccessor.GetType()).ToArray()
        );

        // Create the parameters.
        var parameters = columnMappings
            .Select(m => m.Mapping.RowValueAccessor)
            .ToArray();

        // Get the IL generator.
        var il = constructor.GetILGenerator();

        // Get the field for the disposables, will be
        // used to initialize the disposables
        // as well as populate them.
        var disposablesFieldInfo = baseType
            .GetField(
                nameof(SqlBulkCopyRowMapperBase<T>._disposables)
                , BindingFlags.Instance | BindingFlags.NonPublic
            )
            ?? throw new InvalidOperationException(
                $"Could not find the {nameof(SqlBulkCopyRowMapperBase<T>._disposables)} field " +
                $"on the {baseType.FullName} type."
            );

        // Get the base constructor, there should be just one.
        var baseConstructor = baseType
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single();

        // Call the base.
        il.PushThis();

        // Push the total number of mappings.
        il.PushInt32(columnMappings.Count);

        // Push the number of boxables.
        il.PushInt32(boxables);

        // Call the base constructor.
        il.Emit(OpCodes.Call, baseConstructor);

        // The index.
        var index = 0;

        // We need to initialize all the fields, as well as boxes.
        foreach (var mapping in columnMappings)
        {
            // Increment the index.
            index++;

            // Index is greater than 0.
            Debug.Assert(index > 0);

            // Load "this"
            il.PushThis();

            // Push the argument on the stack.
            // It will be the first, (first mapping), second (second mapping)
            // and so on.
            il.PushArgument(index);

            // Load the field into the appropriate field builder.
            il.Emit(
                OpCodes.Stfld
                , mapping.MapFieldBuilder
            );

            // If boxing, then create a reusable box.
            if (mapping.Boxing is not null)
            {
                // Get the boxing.
                var boxing = mapping.Boxing.Value;

                // Load "this"
                il.PushThis();

                // The reusable box type.
                var reusableBoxType = typeof(ReusableBox<>)
                    .MakeGenericType(boxing.Type);

                // Get the constructor for the reusable box.
                var reusableBoxConstructor = reusableBoxType
                    .GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException(
                        "Could not find a parameterless constructor for the type "
                        + reusableBoxType.FullName
                    );

                // Create the new object.
                il.Emit(OpCodes.Newobj, reusableBoxConstructor);

                // Store at the appropriate field.
                il.Emit(
                    OpCodes.Stfld
                    , boxing.ReusableBoxFieldBuilder
                );

                // The disposable has to be loaded as well.
                // Load this.
                il.PushThis();

                // Load the disposables array onto the stack.
                il.Emit(OpCodes.Ldfld, disposablesFieldInfo);

                // Load the index.  In this particular case, we will decrement the boxables
                // and this will be the index.
                boxables--;

                // The index cannot be less than zero.
                Debug.Assert(boxables >= 0);

                // Push the index onto the stack.
                il.PushInt32(boxables);

                // Need to load the value from the field onto the stack.
                // Load this.
                il.PushThis();
                il.Emit(OpCodes.Ldfld, boxing.ReusableBoxFieldBuilder);

                // Store the element.
                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        // Boxables is zero.
        Debug.Assert(boxables == 0);

        // Return.
        il.Emit(OpCodes.Ret);

        // Return the parameters.
        return parameters;
    }

    private static ReadOnlySpan<int[]> GenerateSwitchBuckets(IReadOnlyList<int> indices)
    {
        // Assertions:
        // There is at least one item
        // The items are distinct
        // The items are in ascending order.
        Debug.Assert(indices.Count > 0);

        // As per:
        // https://github.com/dotnet/roslyn/blob/63019e999de19e9a451e381d6dbdd8f520e2907f/src/Compilers/Core/Portable/CodeGen/SwitchIntegralJumpTableEmitter.cs#L69
        // We want to find anything where the density is greater than 50% for the valid
        // indices in the bucket.

        // We'll move from left to right, building buckets; as we look at what is in
        // the most recent bucket, if the density falls below 50%, we'll start a new bucket.

        // A list of lists will store the buckets.
        // There's at least one item, so there is at least one bucket.
        var buckets = new List<List<int>>() {
            new List<int>()
        };

        // The last values.
        var last = indices[indices.Count - 1];

        // The current bucket.
        var currentBucket = buckets[0];

        // Cycle through the indices.
        foreach (var (index, consumed) in indices.Select((i, n) => (i, n)))
        {
            // Calculate the density across the remainder.  It is the number of indices
            // left (including the current one) divided by the items between first and
            // last values (inclusive).
            var density = (double)(indices.Count - consumed) / (last - index + 1);
            
            // If the density is greater than 50%, then the rest goes
            // in a new bucket.
            // NOTE: even if the previous item was just one off, then the density
            // would HAVE to be above 50% still.
            // If x / n > 50% then (x + 1) / (n + 1) > 50%
            if (density > .5)
            {
                // Is a new bucket created?
                // No if the count is zero.
                var createNewBucket = currentBucket.Count > 0;

                // Take this and the remaining items and add to the buckets.
                // If the current bucket count is zero, then no need
                // to create a new bucket.
                var bucket = createNewBucket ? new List<int>() : currentBucket;
                bucket.AddRange(indices.Skip(consumed));

                // Add the bucket to buckets if it is a new
                // bucket.
                if (createNewBucket)
                    buckets.Add(bucket);

                // Break.
                break;
            }

            // If there are no items in the current bucket, then just
            // add and continue. (1 / 1 = 100% > 50%)
            if (currentBucket.Count == 0)
            {
                // Add to the current bucket and bail.
                currentBucket.Add(index);
                continue;
            }

            // Check the density of the current bucket with the current item
            // if the density is still above 50% then add to the current bucket
            // otherwise, create a new bucket and add to that.
            density = (double)(currentBucket.Count + 1) / (index - currentBucket[0] + 1);

            // If the density is above 50% then add to the current bucket
            // and bail.
            if (density > .5)
            {
                // Add to the current bucket and continue;
                currentBucket.Add(index);
                continue;
            }

            // This needs a new bucket.
            currentBucket = new List<int>() { index };
            buckets.Add(currentBucket);
        }

        // The implementation.
        IEnumerable<int[]> Implementation()
        {
            // Cycle through the buckets.
            foreach (var bucket in buckets!)
            {
                // Anything that has less than two elements is
                // broken down into separate buckets
                // The compiler believes that a branch between
                // two separate statements is better than
                // a jump table.
                // If the bucket length is not 2
                // then yield the bucket as-is.
                if (bucket.Count != 2)
                    yield return bucket.ToArray();
                else
                {
                    // Yield the first and second elements
                    // separately.
                    yield return new int[1] { bucket[0] };
                    yield return new int[1] { bucket[1] };
                }
            }
        }

        // Return the buckets.
        return Implementation()
            .ToArray()
            .AsSpan();
    }

    private static void PushIndexParameter(this ILGenerator il) => il.PushArgument(2);

    private static void WriteBucket(
        this ILGenerator il
        , IReadOnlyDictionary<int, ValidatedColumnMapping> columnMapings
        , ReadOnlySpan<int> bucket
        , Label defaultLabel
    )
    {
        // If there is one item, then jump if it's equal.
        if (bucket.Length == 1)
        {
            // Load the parameter.
            il.PushIndexParameter();
            il.PushInt32(bucket[0]);

            // If equal, jump to the code for the column mapping.
            il.Emit(OpCodes.Beq, columnMapings[bucket[0]].Label);

            // Bail.
            return;
        }

        // We have to load the index parameter regardless.
        il.PushIndexParameter();

        // Get the first value in the bucket.
        var first = bucket[0];

        // If the first item is not 0 then load
        // that value and subtract
        // from the index passed in.
        if (first != 0)
        {
            // Push the first value
            // and subtract.
            il.PushInt32(first);
            il.Emit(OpCodes.Sub);
        }

        // The number of labels for the bucket.
        var count = bucket[^1] - bucket[0] + 1;

        // An array of labels.
        var labels = new Label[count];

        // Enumerate through the first and last items
        // (inclusive).
        for (int i = bucket[0]; i <= bucket[^1]; i++)
        {
            // Try and get the index.  If it exists then
            // push that label, otherwise, push the
            // default label.
            labels[i - first] = columnMapings.TryGetValue(i, out var pair)
                ? pair.Label
                : defaultLabel;
        }

        // Switch.
        il.Emit(OpCodes.Switch, labels);
    }

    private static void WriteBuckets(
        this ILGenerator il
        , IReadOnlyDictionary<int, ValidatedColumnMapping> columnMapings
        , ReadOnlySpan<int[]> buckets
        , Label defaultLabel
        , bool jumpToDefaultIfNoMoreBranches
        , Label? mark
    )
    {
        // The max bucket count before
        // splitting between the two.
        // Assume this is the top level
        // At the top level, process
        // up to three buckets in order.
        var maxBucketCount = 3;

        // If this is marked, then do so here
        // Note this is not the top level
        // so set the max bucket count to two.
        if (mark is not null)
        {
            // Mark the label.
            il.MarkLabel(mark.Value);
            maxBucketCount = 2;
        }

        // If the cout is less than the max bucket count
        // then just fall through all of the cases.
        if (buckets.Length <= maxBucketCount)
        {
            // Cycle through the buckets
            foreach (var bucket in buckets)
                // Write the bucket.
                il.WriteBucket(
                    columnMapings
                    , bucket
                    , defaultLabel
                );

            // If jumping to default if no more
            // branches, then do that here.
            if (jumpToDefaultIfNoMoreBranches)
                il.Emit(OpCodes.Br, defaultLabel);

            // Get out.
            return;
        }

        // The midpoint.
        var mid = (buckets.Length / 2);

        // Split the buckets into left and right.
        var left = buckets[..mid];
        var leftLabel = il.DefineLabel();
        var right = buckets[mid..];
        var rightLabel = il.DefineLabel();

        // Load the last value of the last bucket on the left.
        var breakpoint = left[^1][^1];

        // Load the index parameter
        // and then the breakpoint.
        il.PushIndexParameter();
        il.PushInt32(breakpoint);

        // Pusk the break to the right label if
        // it is greater than.
        il.Emit(OpCodes.Bgt, rightLabel);

        // Emit the left and then the right.
        // Note we want the right most branches
        // to terminate in defaults if
        // there's nothing left.
        il.WriteBuckets(
            columnMapings
            , left
            , defaultLabel
            , false
            , leftLabel
        );
        il.WriteBuckets(
            columnMapings
            , right
            , defaultLabel
            , true
            , rightLabel
        );
    }

    // Implements:
    // public abstract object? Map(T instance, int ordinal);
    private static void ImplementMapMethod<T>(
        this TypeBuilder typeBuilder
        , IReadOnlyList<ValidatedColumnMapping> columnMappings
    )
    {
        // The map method name.
        const string mapMethodName = nameof(SqlBulkCopyRowMapperBase<T>.Map);

        // Get the base method.
        var baseMethodInfo = typeof(SqlBulkCopyRowMapperBase<T>)
            .GetMethod(mapMethodName)
            ?? throw new InvalidOperationException(
                $"Could not find the {mapMethodName} method on the "
                + $"{typeof(SqlBulkCopyRowMapperBase<T>).FullName} type."
            );

        // Get the base method parameter infos.
        var baseMethodParameterInfos = baseMethodInfo
            .GetParameters();

        // Build the method body for the explicit interface
        // implementation from the abstract method, as per
        // https://stackoverflow.com/a/56625342/50776
        MethodBuilder implementationBuilder = typeBuilder
            .DefineMethod(
                mapMethodName
                , MethodAttributes.Public
                | MethodAttributes.ReuseSlot
                | MethodAttributes.Virtual
                | MethodAttributes.HideBySig
                | MethodAttributes.Final
                , CallingConventions.HasThis
                , baseMethodInfo.ReturnType
                , baseMethodInfo
                    .ReturnParameter
                    .GetRequiredCustomModifiers()
                , baseMethodInfo
                    .ReturnParameter
                    .GetOptionalCustomModifiers()
                , baseMethodParameterInfos
                    .Select(pi => pi.ParameterType)
                    .ToArray()
                , baseMethodParameterInfos
                    .Select(pi => pi.GetRequiredCustomModifiers())
                    .ToArray()
                , baseMethodParameterInfos
                    .Select(pi => pi.GetOptionalCustomModifiers())
                    .ToArray()
            );

        // Set the method implementation attribute on the builder
        // to aggressively inline.
        implementationBuilder.SetImplementationFlags(
            MethodImplAttributes.AggressiveInlining
        );

        // Get the il generator.
        var il = implementationBuilder.GetILGenerator();

        // The index for local variables.
        var localIndex = 0;

        // We need a variable of one type of nullable for each truly
        // nullable structure we have.  We don't need one for *each*
        // nullable field, though.
        var nullableVariableByType = (
            from m in columnMappings
            let nb = m.Boxing
            where nb?.Nullable == true
            let b = nb.Value
            group b by b.Type into g
            select g.Key
        )
        .Select((t, i) => (type: t, local: il.DeclareLocal(typeof(Nullable<>).MakeGenericType(t)), index: localIndex++))
        .ToDictionary(p => p.type, p => (p.local, p.index))
        .AsReadOnly();

        // We'll also need one type of each distinct value type
        // across nullable and non nullable types
        var nonNullValueTypeVariableByType = (
            from m in columnMappings
            let nb = m.Boxing
            where nb is not null
            let b = nb.Value
            group b by b.Type into g
            select g.Key
        )
        .Select((t, i) => (type: t, local: il.DeclareLocal(t), index: localIndex++))
        .ToDictionary(p => p.type, p => (p.local, p.index))
        .AsReadOnly();

        // Create the default label.
        var defaultLabel = il.DefineLabel();

        // Get the index buckets.
        var indexBuckets = GenerateSwitchBuckets(
            columnMappings
                .Select(c => c.Mapping.Ordinal)
                .ToArray()
                .AsReadOnly()
        );

        // Set labels for all of the column mappings and map
        // it into a dictionary.
        var columnMappingsByColumnOrdinal = columnMappings
            .Select(m => m with { Label = il.DefineLabel() })
            .ToDictionary(m => m.Mapping.Ordinal)
            .AsReadOnly();

        // Write the IL for switching among the buckets.
        // Terminate the branch if there is only one.
        il.WriteBuckets(
            columnMappingsByColumnOrdinal
            , indexBuckets
            , defaultLabel
            , true
            , default
        );

        // Cycle through the column mappings, ordered by
        // ordinal.
        // We need to resort because these are all structs
        // and the ones with the labels are in the dictionary.
        var orderedMappings = columnMappingsByColumnOrdinal
            .OrderBy(p => p.Key)
            .Select(p => p.Value);

        // Define the label to load null
        // and the end of method.
        var loadNullLabel = il.DefineLabel();
        var endOfMethodLabel = il.DefineLabel();

        // Cycle.
        foreach (var mapping in orderedMappings)
        {
            // Get the label.
            var label = mapping.Label;

            // Mark the label.
            il.MarkLabel(label);

            // Get boxing.
            var nullableBoxing = mapping.Boxing;

            // Push this and the field that has the mapper.
            // TODO: Explore being able to move this out
            // and pushed once in the optimal case.
            // NOTE: This produces an invalid program.
            // Likely it is this code (and not the CLR)
            // that is at fault, but work to better understand
            // and test.
            il.PushThis();
            il.Emit(OpCodes.Ldfld, mapping.MapFieldBuilder);

            // Load the first parameter, this is address
            // of the instance of T (no need to get it's address)
            il.PushArgument(1);

            // Make the call to map.
            // The result is on the top of the stack now.
            il.Emit(OpCodes.Call, mapping.MapFieldBuilderGetMethod);

            // Is there a box field builder?
            if (nullableBoxing is not null)
            {
                // Get boxing.
                var boxing = nullableBoxing.Value;

                // There will *always* be a value.
                // If it's nullable, we have to call
                // HasValue and then branch to load null
                // if it is false.
                if (boxing.Nullable)
                {
                    // Get the local.
                    var local = nullableVariableByType[boxing.Type];

                    // Store in the local, we will need the *address*
                    // to make calls.
                    il.PushStoreLocal(local.index);

                    // Get the has value method and
                    // the value property.
                    // Get the nullable type first.
                    var nullableType = typeof(Nullable<>)
                        .MakeGenericType(boxing.Type);

                    // Get the method infos to call.
                    // Well need to see if there is
                    // a value, and actually get the value.
                    var hasValueGetterMethodInfo = nullableType
                        .GetProperty(
                            nameof(Nullable<int>.HasValue)
                            , BindingFlags.Instance
                            | BindingFlags.Public
                        )
                        ?.GetGetMethod()
                        ?? throw new InvalidOperationException(
                            $"Could not find the get method for the {nameof(Nullable<int>.HasValue)} "
                            + $"property on the type {nullableType.FullName}"
                        );
                    var valueGetterMethodInfo = nullableType
                        .GetProperty(
                            nameof(Nullable<int>.Value)
                            , BindingFlags.Instance
                            | BindingFlags.Public
                        )
                        ?.GetGetMethod()
                        ?? throw new InvalidOperationException(
                            $"Could not find the get method for the {nameof(Nullable<int>.Value)} "
                            + $"property on the type {nullableType.FullName}"
                        );

                    // Load the *address* of the local.
                    il.PushLoadLocalAddress(local.index);

                    // Push the value of HasValue onto
                    // The stack.
                    il.Emit(OpCodes.Call, hasValueGetterMethodInfo);

                    // If the value is false, then jump to load null.
                    il.Emit(OpCodes.Brfalse, loadNullLabel);

                    // Not false, load the address again, get the value.
                    il.PushLoadLocalAddress(local.index);

                    // Push the value of Value onto
                    // The stack.
                    il.Emit(OpCodes.Call, valueGetterMethodInfo);
                }

                // Get the index of the non nullable local.
                var typeLocalIndex = nonNullValueTypeVariableByType[boxing.Type].index;

                // Store in the non nullable variable by type.
                il.PushStoreLocal(typeLocalIndex);

                // Load this and load the boxable field.
                il.PushThis();
                il.Emit(OpCodes.Ldfld, boxing.ReusableBoxFieldBuilder);

                // Load the address back onto the stack.
                il.PushLoadLocalAddress(typeLocalIndex);

                // Make the call to reuse the box.
                il.Emit(OpCodes.Call, boxing.ReusableBoxGetBoxMethodInfo);
            }

            // Get out, the object is on the stack.
            il.Emit(OpCodes.Br, endOfMethodLabel);
        }

        // Default case
        il.MarkLabel(defaultLabel);

        // The throw method name.
        const string createExceptionMethodName = 
            nameof(SqlBulkCopyRowMapperBase<T>.CreateInvalidOrdinalArgumentOutOfRangeException);

        // We want to throw here.
        var throwMethod = typeof(SqlBulkCopyRowMapperBase<T>)
            .GetMethod(
                createExceptionMethodName
                , BindingFlags.Static | BindingFlags.NonPublic
            )
            ?? throw new InvalidOperationException(
                $"Could not find the {createExceptionMethodName} method on the {typeof(SqlBulkCopyRowMapperBase<T>).FullName} type."
            );

        // Load the index.  Second parameter.
        il.PushIndexParameter();

        // Call. This puts the exception on the stack.
        il.Emit(OpCodes.Call, throwMethod);

        // Throw.
        il.Emit(OpCodes.Throw);

        // Loading null for nullable boxes.
        il.MarkLabel(loadNullLabel);
        il.Emit(OpCodes.Ldnull);

        // End of method, return.
        il.MarkLabel(endOfMethodLabel);
        il.Emit(OpCodes.Ret);
    }

    #endregion

    #region Build

    public static ISqlBulkCopyRowMapper<T> CreateSqlBulkCopyMapper<T>(
        this IReadOnlyCollection<SqlBulkCopyMapperColumnMapping> columnMappings
    )
    {
        // Validate the column mappings.
        var validated = columnMappings.ValidateColumnMappings<T>();

        // Create the type builder.
        var typeBuilder = CreateTypeBuilder<T>();

        // Create the fields to store everything.
        typeBuilder.CreateFields(validated);

        // Create the constructor.
        var parameters = typeBuilder.CreateConstructor<T>(validated);

        // Implement the map method.
        typeBuilder.ImplementMapMethod<T>(validated);

        // Create the type.
        var type = typeBuilder.CreateType();

        // The instance.
        return (ISqlBulkCopyRowMapper<T>?) Activator.CreateInstance(
            type
            , parameters
        )
            ?? throw new InvalidOperationException(
                $"The call to create an implementation of {typeof(ISqlBulkCopyRowMapper<T>).FullName} "
                + "returned null."
            );
    }

    #endregion
}

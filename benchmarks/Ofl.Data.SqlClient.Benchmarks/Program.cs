using BenchmarkDotNet.Running;
using Ofl.Data.SqlClient.Benchmarks;

//static async Task Run()
//{
//    // Create the benchmark.
//    var bm = new SqlBulkCopyBenchmarks<int>()
//    {
//        BatchSize = 100_000
//        , EnableStreaming = false
//        , Rows = 10_000
//    };

//    // Set up the iteration.
//    await bm.SetupGlobalAsync().ConfigureAwait(false);
//    bm.SetupIteration();

//    // Run the benchmark
//    var result = await bm
//        .Ofl()
//        .ConfigureAwait(false);

//    // Wait (for memory dump)
//    Console.WriteLine($"{nameof(bm.Ofl)} returned: {{0}}", result);
//    Console.WriteLine("Press enter to continue.");
//    Console.ReadLine();

//    // Cleanup.
//    bm.CleanupIteration();
//    await bm.CleanupGlobalAsync().ConfigureAwait(false);
//}

//BenchmarkRunner.Run<SqlBulkCopyMapperColumnMappingExtensionsBenchmarks>();
//await Run().ConfigureAwait(false);
//BenchmarkRunner.Run<SqlBulkCopyBenchmarks<int>>();
BenchmarkRunner.Run(
    new Type[] {
        typeof(MapperBenchmarks<ReferenceInputType<int>, int>)
        , typeof(MapperBenchmarks<ValueInputType<int>, int>)
    }
);

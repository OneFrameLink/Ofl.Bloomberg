using BenchmarkDotNet.Running;
using Ofl.Bloomberg.BackOffice.Benchmarks;

static async Task Run()
{
    // Create the benchmark.
    var bm = new EnumerateDataSectionBenchmark {
        FileToProcess = "credit_risk.dif"
    };

    // Set up the iteration.
    bm.SetupIteration();

    // Run the benchmark
    var count = await bm
        .FileStreamAsyncForEach()
        .ConfigureAwait(false);

    // Wait (for memory dump)
    Console.WriteLine("{0} rows read.", count);
    Console.WriteLine("Press enter to continue.");
    Console.ReadLine();

    // Cleanup.
    bm.CleanupIteration();
}

BenchmarkRunner.Run<EnumerateDataSectionBenchmark>();
//await Run().ConfigureAwait(false);


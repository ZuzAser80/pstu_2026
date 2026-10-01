using Xunit;

// Organisation.cs writes diagnostics straight to System.Console, so capturing it means
// swapping the process-wide Console.Out. xUnit runs test classes in parallel, which would
// let one test steal another test's stdout, so the whole assembly runs sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
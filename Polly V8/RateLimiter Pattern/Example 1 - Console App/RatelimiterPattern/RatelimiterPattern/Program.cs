using RateLimiterPattern;

namespace RatelimiterPattern
{
    internal class Program
    { 
        static async Task Main(string[] args)
        {
            MyClass myClass = new MyClass();

            // Random task IDs will get executed and queued up
            await Parallel.ForEachAsync(Enumerable.Range(1, 10), async (id, cancellationToken) =>
            {
                await myClass.PrintStmt(id);
            });

            Console.WriteLine("\nAll tasks completed.");
        }
    }
}

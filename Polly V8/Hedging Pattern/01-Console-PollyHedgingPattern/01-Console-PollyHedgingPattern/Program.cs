namespace PollyHedgingDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("\n***\tPolly Hedging Pattern Demo\t***\n");

            Services services = new Services();
            

            for (int i = 1; i <= 10; i++) 
            {
                PollyStrategies pollyStrategies = new PollyStrategies(i, services);
                string result = await pollyStrategies.HedgingStrategy.ExecuteAsync<string>(async (ct) => await services.PrimaryService(i));
                Console.WriteLine($"Task {i} result: {result}\n");

            }

            Console.ReadKey();
        }
    }
}
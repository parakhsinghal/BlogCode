namespace PollyHedgingDemo
{
    public class Services
    {
        public async Task<string> PrimaryService(int id)
        {
            int delay = Random.Shared.Next(500, 2000);
            Console.WriteLine($"Task id: {id} - Primary service call delay: {delay} ms");

            await Task.Delay(delay);
            return ($"Primary service completed at {DateTime.Now}");


        }

        public async Task<string> BackupService(int id)
        {
            int delay = 500;
            await Task.Delay(delay);

            Console.WriteLine($"Task id: {id} - Backup service call delay: {delay} ms");
            return ($"Backup service completed at {DateTime.Now}");
        }
    }
}

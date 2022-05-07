namespace WeatherBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new Client(Config.Token);

            client.StartEcho();
        }
    }
}
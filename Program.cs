namespace WeatherBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new Client(Config.Token);

            client.BotStart();
        }
    }
}
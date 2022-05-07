using System;

namespace WeatherBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new Client(Config.Token);

            Console.WriteLine("Welcome. Please select option:");
            Console.WriteLine("0. Exit program");
            Console.WriteLine("1. Start engine");

            while (true)
            {
                var action = Console.ReadLine();
                switch (action)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye.");
                        return;
                    case "1":
                        client.StartEcho();
                        break;
                    default:
                        Console.WriteLine("\nUnknown command! Try again.");
                        Console.WriteLine("\nPlease select option:");
                        Console.WriteLine("0. Exit program");
                        Console.WriteLine("1. Start engine");
                        continue;
                }

                break;
            }
        }
    }
}
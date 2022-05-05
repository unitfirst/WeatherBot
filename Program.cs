using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace WeatherBot
{
    internal class Program
    {
        private static readonly string token = "5314975107:AAGYAQgQuWhDNslmuT-JH_-kQdGH6PJEoL0";
        private static TelegramBotClient _client = new TelegramBotClient(token);

        private static Task HandleErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static Task HandleUpdatesAsync(ITelegramBotClient client, Update update, CancellationToken cts)
        {
            throw new NotImplementedException();
        }


        public static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            _client.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await _client.GetMeAsync(cancellationToken: cts.Token);

            Console.WriteLine($"Start listening: {me.Username}");
            Console.ReadLine();
        }
    }
}
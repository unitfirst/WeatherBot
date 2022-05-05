using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

        private static async Task HandleUpdatesAsync(ITelegramBotClient client, Update update, CancellationToken cts)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(client, update.Message);
                return;
            }
        }

        private static async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text == "/start")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Hello!");
            }
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

            cts.Cancel();
        }
    }
}
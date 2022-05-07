using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WeatherBot
{
    public class Client
    {
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cts;
        private readonly ReceiverOptions _receiverOptions;
        private string _token;

        public Client(string token)
        {
            _client = new TelegramBotClient(token);
            _cts = new CancellationTokenSource();
            _token = token;
            _receiverOptions = new ReceiverOptions();
        }

        public void BotStart()
        {
            _client.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                _receiverOptions,
                _cts.Token);

            BotCheck();

            Console.ReadLine();
            BotStop();
        }

        private async void BotCheck()
        {
            var me = await _client.GetMeAsync(_cts.Token);
            
            Console.WriteLine($"BotStart listening: {me.Username}");
        }

        private void BotStop()
        {
            _cts.Cancel();
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cts)
        {
            var exceptionMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Error telegram API\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(exceptionMessage);
            return Task.CompletedTask;
        }

        private async Task HandleUpdatesAsync(ITelegramBotClient client, Update update, CancellationToken cts)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(client, update.Message);
            }
        }

        private async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text == "/start") await client.SendTextMessageAsync(message.Chat.Id, "Hello!");
        }
    }
}
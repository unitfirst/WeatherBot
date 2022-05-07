using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WeatherBot.Command.Commands;

namespace WeatherBot
{
    public class Client
    {
        private readonly string _token;
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cts;
        private readonly ReceiverOptions _receiverOptions;
        private readonly List<Command.Command> _commands;

        public Client(string token)
        {
            _token = token;
            _client = new TelegramBotClient(token);
            _cts = new CancellationTokenSource();
            _receiverOptions = new ReceiverOptions() {AllowedUpdates = { }};
            _commands = new List<Command.Command>();
        }

        public void StartEcho()
        {
            _client.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                _receiverOptions,
                _cts.Token);

            CreateList();
            CheckEcho();

            Console.ReadLine();
            _commands.Clear();
            StopEcho();
        }

        private async void CheckEcho()
        {
            var me = await _client.GetMeAsync(_cts.Token);

            Console.WriteLine($"StartEcho listening: {me.Username}");
        }

        private void StopEcho()
        {
            _cts.Cancel();
        }

        private void CreateList()
        {
            _commands.Add(new GetHelp());
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

        private Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text != null)
            {
                Console.WriteLine($"{message.Chat.Id}\t{message.From?.Username}\t{message.Text}");

                foreach (var msg in _commands)
                {
                    if (msg.Contains(message.Text))
                    {
                        msg.Execute(message, client);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
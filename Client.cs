using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cts;
        private readonly ReceiverOptions _receiverOptions;
        private readonly List<Command.Command> _commands;
        private readonly Config _config = new Config();
        private WeatherResponse _weatherResponse;

        public Client(string token)
        {
            _client = new TelegramBotClient(token);
            _cts = new CancellationTokenSource();
            _receiverOptions = new ReceiverOptions() {AllowedUpdates = { }};
            _commands = new List<Command.Command>();
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
            if (update.Type == UpdateType.Message &&
                (update.Message?.Text != null || update.Message?.Location != null))
            {
                await HandleMessage(client, update.Message);
            }
        }

        private async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text != null)
            {
                Console.WriteLine(
                    $"{message.Chat.Id}" +
                    $"\t{message.From?.Username}" +
                    $"\t{message.Text}");

                foreach (var msg in _commands)
                {
                    if (msg.Contains(message.Text))
                    {
                        msg.Execute(message, client);
                    }
                }
            }

            if (message.Type == MessageType.Location)
            {
                Console.WriteLine(
                    $"{message.Chat.Id}" +
                    $"\t{message.From?.Username}" +
                    $"\t{message.Location?.Latitude}" +
                    $"\t{message.Location?.Longitude}");
            }

            GetWeather(message);

            await client.SendTextMessageAsync(
                message.Chat.Id,
                $"\nTemperature in {_weatherResponse.Name}" +
                $"\n{_weatherResponse.Main.Temp} °C" +
                $"\n{_weatherResponse.Main.Feels_Like} °C");

            Console.WriteLine(
                $"{_weatherResponse.Name}" +
                $"\t{_weatherResponse.Main.Temp}" +
                $"\t{_weatherResponse.Main.Feels_Like}");
        }

        public void StartEcho()
        {
            _client.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                _receiverOptions,
                _cts.Token);

            AddCommand();
            CheckEcho();

            Console.ReadLine();
            _commands.Clear();
            StopEcho();
        }

        private async void CheckEcho()
        {
            var me = await _client.GetMeAsync(_cts.Token);

            Console.WriteLine($"Start listening: {me.Username}");
        }

        private void StopEcho()
        {
            _cts.Cancel();
        }

        private void AddCommand()
        {
            _commands.Add(new GetHelp());
        }

        private HttpWebRequest RequestType(Message message)
        {
            return (message.Type == MessageType.Text)
                ? (HttpWebRequest) WebRequest.Create(_config.GetUrl(message.Text))
                : (HttpWebRequest) WebRequest.Create(_config.GetUrl(message.Location));
        }

        private void GetWeather(Message message)
        {
            try
            {
                string response;

                var webResponse = (HttpWebResponse) RequestType(message).GetResponse();
                using (var sr =
                    new StreamReader(webResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    response = sr.ReadToEnd();
                }

                _weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Exception!");
                return;
            }
        }
    }
}
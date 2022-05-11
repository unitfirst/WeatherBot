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
                return;
            }
        }

        private Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text != null)
            {
                if (message.Type == MessageType.Location)
                {
                    Console.WriteLine(
                        $"{message.Chat.Id}" +
                        $"\t{message.From?.Username}" +
                        $"\t{message.Location?.Latitude}" +
                        $"\t{message.Location?.Longitude}");
                }

                Console.WriteLine(
                    $"{message.Chat.Id}" +
                    $"\t{message.From?.Username}" +
                    $"\t{message.Text}");

                foreach (var msg in _commands)
                {
                    if (msg.Contains(message.Text))
                    {
                        msg.Execute(message, client);
                        return Task.CompletedTask;
                    }
                }
            }

            try
            {
                GetWeather(client, message);
            }
            catch (Exception e)
            {
                Console.WriteLine("HandleMessage exception");
            }

            message = null;
            return Task.CompletedTask;
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
            StopEcho();
        }

        private async void CheckEcho()
        {
            var me = await _client.GetMeAsync(_cts.Token);

            Console.WriteLine($"Start listening: {me.Username}");
        }

        public void StopEcho()
        {
            _commands.Clear();
            _cts.Cancel();
        }

        private void AddCommand()
        {
            _commands.Add(new GetHelp());
            _commands.Add(new GetStart());
        }

        private HttpWebRequest RequestType(Message message)
        {
            return (message.Type == MessageType.Text)
                ? (HttpWebRequest) WebRequest.Create(_config.GetUrl(message.Text))
                : (HttpWebRequest) WebRequest.Create(_config.GetUrl(message.Location));
        }

        private Task GetWeather(ITelegramBotClient client, Message message)
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

                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                var name = weatherResponse.Name;
                var temp = weatherResponse.Main.Temp;
                var feels = weatherResponse.Main.Feels_Like;

                client.SendTextMessageAsync(
                    message.Chat.Id,
                    $"\nTemperature in {name}" +
                    $"\nTemp:\t{Math.Round(temp)} °C" +
                    $"\nFeels like:\t{Math.Round(feels)} °C");
                
                Console.WriteLine(
                    $"{name}" +
                    $"\t{temp}" +
                    $"\t{feels}");
            }
            catch (WebException)
            {
                client.SendTextMessageAsync(message.Chat.Id, "\nSorry. I Dont know what is a place.");
                Console.WriteLine("owm.org error");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
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
        private readonly string _token;
        private readonly string lang = "ru";
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cts;
        private readonly ReceiverOptions _receiverOptions;
        private readonly List<Command.Command> _commands;

        private string NameOfCity { get; set; }
        private float TempOfCity { get; set; }
        private float FeelsLike { get; set; }

        public Client(string token)
        {
            _token = token;
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
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(client, update.Message);
            }
            else if (update.Type == UpdateType.Message && update.Message?.Location != null)
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

                if (message.Type == MessageType.Text)
                {
                    ResponseByName(message.Text);
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        $"\nTemperature: {NameOfCity}" +
                        $"\n{TempOfCity} °C" +
                        $"\n{FeelsLike} °C");
                }
            }
            else if (message.Type == MessageType.Location)
            {
                Console.WriteLine(
                    $"{message.Chat.Id}" +
                    $"\t{message.From?.Username}" +
                    $"\t{message.Location.Latitude}" +
                    $"\t{message.Location.Longitude}");

                ResponseByGeo(message.Location);
            }

            await client.SendTextMessageAsync(
                message.Chat.Id,
                $"\nTemperature: {NameOfCity}" +
                $"\n{TempOfCity} °C" +
                $"\n{FeelsLike} °C");

            Console.WriteLine($"{NameOfCity}\t{TempOfCity}\t{FeelsLike}");
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

        private void ResponseByName(string cityName)
        {
            try
            {
                var url =
                    $"https://api.openweathermap.org/data/2.5/weather" +
                    $"?q={cityName}" +
                    $"&unit=metric" +
                    $"&appid={Config.APIKey}" +
                    $"&lang={lang}";

                var webRequest = (HttpWebRequest) WebRequest.Create(url);
                var webResponse = (HttpWebResponse) webRequest?.GetResponse();

                string response;
                using (var sr =
                    new StreamReader(webResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    response = sr.ReadToEnd();
                }

                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                if (weatherResponse != null)
                {
                    NameOfCity = weatherResponse.Name;
                    TempOfCity = weatherResponse.Main.Temp;
                    FeelsLike = weatherResponse.Main.Feels_Like;
                }
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Exception!");
                return;
            }
        }

        private void ResponseByGeo(Location location)
        {
            try
            {
                var url =
                    $"https://api.openweathermap.org/data/2.5/weather" +
                    $"?lat={location.Latitude}" +
                    $"&lon={location.Longitude}" +
                    $"&appid={Config.APIKey}";

                var webRequest = (HttpWebRequest) WebRequest.Create(url);
                var webResponse = (HttpWebResponse) webRequest?.GetResponse();

                string response;
                using (var sr =
                    new StreamReader(webResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    response = sr.ReadToEnd();
                }

                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                if (weatherResponse != null)
                {
                    var lat = weatherResponse.Coord.lat;
                    var lon = weatherResponse.Coord.lon;
                }
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Exception!");
                return;
            }
        }
    }
}
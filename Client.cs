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

        private string _nameOfCity { get; set; }
        private float _tempOfCity { get; set; }
        private float _feelsLike { get; set; }

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
        }

        private async Task HandleMessage(ITelegramBotClient client, Message message)
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

                WeatherResponseByName(message.Text);
                await client.SendTextMessageAsync(
                    message.Chat.Id, $"\nTemperature: {_nameOfCity} \n{_tempOfCity} °C\n{_feelsLike} °C");

                Console.WriteLine($"{_nameOfCity}\t{_tempOfCity}\t{_feelsLike}");
            }
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

            Console.WriteLine($"Start listening: {me.Username}");
        }

        private void StopEcho()
        {
            _cts.Cancel();
        }

        private void CreateList()
        {
            _commands.Add(new GetHelp());
        }

        private async void WeatherResponseByName(string cityName)
        {
            try
            {
                var url =
                    $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&unit=metric&appid={Config.APIKey}&lang={lang}";

                var webRequest = (HttpWebRequest) WebRequest.Create(url);
                var webResponse = (HttpWebResponse) webRequest?.GetResponse();

                string response;
                using (var sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                if (weatherResponse != null)
                {
                    _nameOfCity = weatherResponse.Name;
                    _tempOfCity = weatherResponse.Main.Temp;
                    _feelsLike = weatherResponse.Main.Feels_Like;
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
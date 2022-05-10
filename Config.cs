using System;
using Telegram.Bot.Types;

namespace WeatherBot
{
    public class Config
    {
        private string _url;
        private string Token { get; set; }

        private string ApiKey { get; set; }

        public string Lang
        {
            get => "en";
            set => Lang = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string GetToken()
        {
            return "5314975107:AAGYAQgQuWhDNslmuT-JH_-kQdGH6PJEoL0";
        }

        private static string GetApi()
        {
            return "3df6bf26591e4e5f1ee9a77b9fc19052";
        }

        public string GetUrl(Location location)
        {
            var url =
                "https://api.openweathermap.org/data/2.5/weather" +
                $"?lat={location.Latitude}" +
                $"&lon={location.Longitude}" +
                $"&appid={GetApi()}";

            return url;
        }

        public string GetUrl(string message)
        {
            _url = "https://api.openweathermap.org/data/2.5/weather" +
                   $"?q={message}" +
                   "&unit=metric" +
                   $"&appid={GetApi()}" +
                   $"&lang={Lang}";

            return _url;
        }
    }
}
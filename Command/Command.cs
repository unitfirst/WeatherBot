using System.Net.Configuration;
using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WeatherBot.Command
{
    public abstract class Command
    {
        public abstract string[] Names { get; set; }
        public abstract void Execute(Message message, ITelegramBotClient client);

        public bool Contains(string message)
        {
            foreach (var msg in Names)
            {
                if (msg.Contains(message))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
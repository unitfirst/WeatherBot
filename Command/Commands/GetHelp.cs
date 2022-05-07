using Telegram.Bot;
using Telegram.Bot.Types;

namespace WeatherBot.Command.Commands
{
    public class GetHelp : Command
    {
        public override string[] Names { get; set; } = new string[] {"/help", "help", "how to"};

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.SendTextMessageAsync(message.Chat.Id, "Sample help text...");
        }
    }
}
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WeatherBot.Command.Commands
{
    public class GetStart : Command
    {
        public override string[] Names { get; set; } = new string[] {"/start", "start", "go"};

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.SendTextMessageAsync(message.Chat.Id, "Welcome. I am Hot9000. I know what weather now in your place.");
        }
    }
}
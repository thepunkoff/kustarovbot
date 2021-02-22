using System.Linq;
using System.Net.Mime;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class CommandParser
    {
        private readonly TelegramBotClient _botClient;

        public CommandParser(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public ICommand ParseCommand(Message message)
        {
            var split = message.Text.Split(" ");
            var command = split[0];
            var args = split.Skip(1).ToArray();

            return command switch
            {
                "/status" => new StatusCommand(_botClient, message),
                "/target" => new SetTargetCommand(_botClient, message, args),
                "/iambusy" => new SendMenuCommand(_botClient, message, "iambusy", false),
                "/iambusy/changeText" => new IAmBusyChangeTextCommand(_botClient, message, args),
                _ => new SendMessageCommand(_botClient, message, $"Такой команды не существует: '{message.Text}'")
            };
        }
    }
}
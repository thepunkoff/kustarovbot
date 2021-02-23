using System.Linq;
using System.Net.Mime;
using KustarovBotTelegramUI.State;
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

        public ICommand ParseCommand(ChatId chatId, string text, int originalMessageId = 0)
        {
            var split = text.Split(" ");
            var command = split[0];
            var args = split.Skip(1).ToArray();

            return command switch
            {
                "/status" => new StatusCommand(_botClient,chatId),
                "/menu" => new SendMenuCommand(_botClient, chatId, args[0], originalMessageId),
                "/scheduleMenu" => new SendScheduleMenuCommand(_botClient, chatId, originalMessageId),
                "/message" => new SendMessageCommand(_botClient, chatId, args[0]),
                "/procedure" => new StartProcedureCommand(args[0]),
                "/target" => new SetTargetCommand(_botClient, chatId, args[0]),
                "/iambusy" => new SendMenuCommand(_botClient, chatId, "iambusy", originalMessageId),
                "/changeDay" => new IAmBusyChangeDayCommand(_botClient, chatId, args[0], bool.Parse(args[1]), originalMessageId),
                "/changeSchedule" => new IAmBusyChangeScheduleCommand(_botClient, chatId, new Schedule
                {
                    Monday = true,
                    Tuesday = true,
                    Wednesday = true,
                    Thursday= true,
                    Friday= true,
                    Saturday = true,
                    Sunday = true,
                }),
                _ => new SendRawMessageCommand(_botClient, chatId, $"Такой команды не существует: '{text}'")
            };
        }
    }
}
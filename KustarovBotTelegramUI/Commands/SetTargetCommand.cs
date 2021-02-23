using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SetTargetCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly string _uriString;

        public string DebugName { get; }

        public SetTargetCommand(TelegramBotClient botClient, ChatId chatId, string text)
        {
            DebugName = nameof(SetTargetCommand);
            _botClient = botClient;
            _chatId = chatId;
            _uriString = text;
        }


        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            TelegramKustarovBotUI.Target = new Uri(_uriString);
            await _botClient.SendTextMessageAsync(_chatId, $"Консоль теперь направлена на бота по адресу '{_uriString}'");
        }
    }
}
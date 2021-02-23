using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SendRawMessageCommand : ICommand
    {
        private readonly ChatId _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly string _text;
        public string DebugName { get; } = nameof(SendRawMessageCommand);

        public SendRawMessageCommand(TelegramBotClient botClient, ChatId chatId, string text)
        {
            _chatId = chatId;
            _botClient = botClient;
            _text = text;
        }

        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            await _botClient.SendTextMessageAsync(_chatId, _text);
        }
    }
}
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SendMessageCommand : ICommand
    {
        private readonly Message _message;
        private readonly TelegramBotClient _botClient;
        private readonly string _text;
        public string DebugName { get; init; }

        public SendMessageCommand(TelegramBotClient botClient, Message message, string text)
        {
            DebugName = nameof(SendMessageCommand);
            _message = message;
            _botClient = botClient;
            _text = text;
        }


        public async Task Run()
        {
            await _botClient.SendTextMessageAsync(_message.Chat.Id, _text);
        }
    }
}
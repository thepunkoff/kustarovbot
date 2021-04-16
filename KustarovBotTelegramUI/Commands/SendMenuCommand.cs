using System.Threading.Tasks;
using KustarovBotTelegramUI.Extensions;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SendMenuCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string SendMenu = "sendmenu";

        private readonly ChatId _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly string _menuId;
        private readonly int _originalMessageId;
        public string DebugName { get; }

        public SendMenuCommand(TelegramBotClient botClient, ChatId chatId, string menuId, int originalMessageId = 0)
        {
            DebugName = nameof(SendMenuCommand);
            _botClient = botClient;
            _menuId = menuId;
            _chatId = chatId;
            _originalMessageId = originalMessageId;
        }


        public async Task Run()
        {
            var menu = Resources.GetMenu(_menuId);
            var markup = menu.Rows.MakeMarkup();
            
            Logger.Trace($"[{SendMenu}] sending {_menuId}");

            if (_originalMessageId != 0)
                await _botClient.EditMessageTextAsync(_chatId, _originalMessageId, menu.Text, replyMarkup: markup);
            else
                await _botClient.SendTextMessageAsync(_chatId, menu.Text, replyMarkup: markup);
        }
    }
}
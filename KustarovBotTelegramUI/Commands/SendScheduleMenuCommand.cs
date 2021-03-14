using System.Threading.Tasks;
using KustarovBotTelegramUI.Extensions;
using KustarovBotTelegramUI.Menus;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SendScheduleMenuCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string SendScheduleMenu = "sendschedulemenu";

        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly int _originalMessageId;
        public string DebugName { get; } = nameof(SendScheduleMenuCommand);
        
        public SendScheduleMenuCommand(TelegramBotClient botClient, ChatId chatId, int originalMessageId = 0)
        {
            DebugName = nameof(SendMenuCommand);
            _botClient = botClient;
            _chatId = chatId;
            _originalMessageId = originalMessageId;
        }
        
        public async Task Run()
        {
            Logger.Trace($"[{SendScheduleMenu}] running '{DebugName}' command");
            var menu = await Menu.CreateActualScheduleMenu();
            var markup = menu.Rows.MakeMarkup();
            
            if (_originalMessageId != 0)
                await _botClient.EditMessageTextAsync(_chatId, _originalMessageId, menu.Text, replyMarkup: markup);
            else
                await _botClient.SendTextMessageAsync(_chatId, menu.Text, replyMarkup: markup);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using KustarovBotTelegramUI.Menus;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace KustarovBotTelegramUI.Commands
{
    public class SendMenuCommand : ICommand
    {
        private readonly Message _message;
        private readonly TelegramBotClient _botClient;
        private readonly string _menuId;
        public string DebugName { get; }

        public SendMenuCommand(TelegramBotClient botClient, Message message, string menuId, bool edit)
        {
            DebugName = nameof(SendMenuCommand);
            _message = message;
            _botClient = botClient;
            _menuId = menuId;
        }


        public async Task Run()
        {
            var menu = Resources.GetMenu(_menuId);
            var markup = MakeMakup(menu.Rows);
            await _botClient.SendTextMessageAsync(_message.Chat.Id, menu.Text, replyMarkup: markup);
        }

        private static InlineKeyboardMarkup MakeMakup(IEnumerable<Row> menuRows)
        {
            return new (menuRows.Select(row => row.Buttons.Select(button => new InlineKeyboardButton
            {
                Text = button.Text,
                CallbackData = button.Command,
            }).ToList()).ToList());
        }
    }
}
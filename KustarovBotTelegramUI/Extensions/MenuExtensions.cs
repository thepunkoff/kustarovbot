using System.Collections.Generic;
using System.Linq;
using KustarovBotTelegramUI.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace KustarovBotTelegramUI.Extensions
{
    public static class MenuExtensions
    {
        public static InlineKeyboardMarkup MakeMarkup(this IEnumerable<Row> menuRows)
        {
            return new (menuRows.Select(row => row.Buttons.Select(button => new InlineKeyboardButton
            {
                Text = button.Text,
                CallbackData = button.Commands is not null ? string.Join(';', button.Commands) : "null",
            }).ToList()).ToList());
        }
    }
}
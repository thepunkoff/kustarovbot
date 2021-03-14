using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using KustarovBotTelegramUI.Extensions;
using KustarovBotTelegramUI.Menus;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        private readonly bool _edit;
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
            Logger.Trace($"[{SendMenu}] running '{DebugName}' command");
            var menu = Resources.GetMenu(_menuId);
            var markup = menu.Rows.MakeMarkup();
            
            if (_originalMessageId != 0)
                await _botClient.EditMessageTextAsync(_chatId, _originalMessageId, menu.Text, replyMarkup: markup);
            else
                await _botClient.SendTextMessageAsync(_chatId, menu.Text, replyMarkup: markup);
        }
    }
}
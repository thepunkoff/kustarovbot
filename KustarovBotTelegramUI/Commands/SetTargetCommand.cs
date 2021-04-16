using System;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SetTargetCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string SetTarget = "settarget";

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
            Logger.Trace($"[{SetTarget}] setting target to be {_uriString}");
            TelegramKustarovBotUI.Target = new Uri(_uriString);
            await _botClient.SendTextMessageAsync(_chatId, $"Консоль теперь направлена на бота по адресу '{_uriString}'");
        }
    }
}
﻿using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SendMessageCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string SendMessage = "sendmessage";

        private readonly ChatId _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly string _text;
        private readonly int _orginalMessageId;
        public string DebugName { get; }

        public SendMessageCommand(TelegramBotClient botClient, ChatId chatId, string messageId, int originalMessageId = 0)
        {
            DebugName = nameof(SendMessageCommand);
            _chatId = chatId;
            _botClient = botClient;
            _orginalMessageId = originalMessageId;
            _text = Resources.GetMessageText(messageId);
        }


        public async Task Run()
        {
            var edit = _orginalMessageId != 0;
            Logger.Trace($"[{SendMessage}] {(edit ? "editing" : "sending")} message");

            if (edit)
                await _botClient.EditMessageTextAsync(_chatId, _orginalMessageId, _text);    
            else
                await _botClient.SendTextMessageAsync(_chatId, _text);
        }
    }
}
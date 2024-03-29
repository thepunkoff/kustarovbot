﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class IAmBusyChangeTextCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly string _text;
        public string DebugName { get; }

        public IAmBusyChangeTextCommand(TelegramBotClient botClient, ChatId chatId, string text)
        {
            DebugName = nameof(IAmBusyChangeTextCommand);
            _botClient = botClient;
            _chatId = chatId;
            _text = text;
        }
        
        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += $"iambusy/changeText";
            ub.Query = $"text={_text}";

            Console.WriteLine($"sending request to {ub}");
            var request = WebRequest.CreateHttp(ub.ToString());
            request.Method = "GET";

            WebResponse response = null;
            try
            {
                var webResponse = await request.GetResponseAsync();
                var httpResponse = (HttpWebResponse) webResponse;
                Console.WriteLine($"bot returned status code '{httpResponse.StatusCode}'");
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                    await _botClient.SendTextMessageAsync(_chatId, "Текст ответа изменен!");
            }
            catch (WebException wex)
            {
                var errorResponse = (HttpWebResponse) wex.Response;
                Console.WriteLine($"bot returned status code '{errorResponse.StatusCode}'");
                await _botClient.SendTextMessageAsync(_chatId, "Произошла ошибка.");
            }
        }
    }
}
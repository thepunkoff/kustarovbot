using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class StatusCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;

        public string DebugName { get; }

        public StatusCommand(TelegramBotClient botClient, ChatId chatId)
        {
            DebugName = nameof(SetTargetCommand);
            _botClient = botClient;
            _chatId = chatId;
        }

        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "status";

            Console.WriteLine($"sending request to {ub}");
            var request = WebRequest.Create(ub.ToString());
            request.Method = "GET";

            using var response = await request.GetResponseAsync();
            var httpResponse = (HttpWebResponse) response;

            Console.WriteLine($"bot returned status '{httpResponse.StatusCode}'");
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                await _botClient.SendTextMessageAsync(_chatId, "Произошла ошибка.");
            }
            else
            {
                await using var responseStream = response.GetResponseStream();
                var streamReader = new StreamReader(responseStream);
                var status = await streamReader.ReadToEndAsync();
                await _botClient.SendTextMessageAsync(_chatId, $"Статус: '{status}'");
            }
        }
    }
}
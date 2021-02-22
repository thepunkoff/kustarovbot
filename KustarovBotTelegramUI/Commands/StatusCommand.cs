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
        private readonly Message _message;

        public string DebugName { get; }

        public StatusCommand(TelegramBotClient botClient, Message message)
        {
            DebugName = nameof(SetTargetCommand);
            _botClient = botClient;
            _message = message;
        }

        public async Task Run()
        {
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
                await _botClient.SendTextMessageAsync(_message.Chat.Id, "Произошла ошибка.");
            }
            else
            {
                await using var responseStream = response.GetResponseStream();
                var streamReader = new StreamReader(responseStream);
                var status = await streamReader.ReadToEndAsync();
                await _botClient.SendTextMessageAsync(_message.Chat.Id, $"Статус: '{status}'");
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class StatusCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Status = "status";

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
            Logger.Trace($"[{Status}] running '{DebugName}' command");
            
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "status";

            Logger.Trace($"[{Status}] sending request to {ub}");
            var request = WebRequest.Create(ub.ToString());
            request.Method = "GET";

            try
            {
                using var response = await request.GetResponseAsync();
                var httpResponse = (HttpWebResponse) response;
                await using var responseStream = response.GetResponseStream();
                Logger.Trace($"[{Status}] bot returned status '{httpResponse.StatusCode}'");
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var streamReader = new StreamReader(responseStream);
                    var status = await streamReader.ReadToEndAsync();
                    await _botClient.SendTextMessageAsync(_chatId, $"Статус: '{status}'");
                }
            }
            catch (WebException wex)
            {
                Logger.Warn($"[{Status}] web exception occured\n" + wex);
                if (wex.Message.Contains("refused"))
                    await _botClient.SendTextMessageAsync(_chatId, "Статус: 'timeout'");
                else
                    await _botClient.SendTextMessageAsync(_chatId, "Статус: 'not ok'");
            }
        }
    }
}
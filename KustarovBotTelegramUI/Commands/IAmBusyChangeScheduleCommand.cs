using System;
using System.Net;
using System.Threading.Tasks;
using KustarovBotTelegramUI.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class IAmBusyChangeScheduleCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly Schedule _newSchedule;
        public string DebugName { get; } = nameof(IAmBusyChangeScheduleCommand);

        public IAmBusyChangeScheduleCommand(TelegramBotClient botClient, ChatId chatId, Schedule newSchedule)
        {
            _botClient = botClient;
            _chatId = chatId;
            _newSchedule = newSchedule;
        }
        
        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "iambusy/changeSchedule";
            ub.Query += $"?monday={_newSchedule.Monday}";
            ub.Query += $"&tuesday={_newSchedule.Tuesday}";
            ub.Query += $"&wednesday={_newSchedule.Wednesday}";
            ub.Query += $"&thursday={_newSchedule.Thursday}";
            ub.Query += $"&friday={_newSchedule.Friday}";
            ub.Query += $"&saturday={_newSchedule.Saturday}";
            ub.Query += $"&sunday={_newSchedule.Sunday}";

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
                    await new SendScheduleMenuCommand(_botClient, _chatId).Run();
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
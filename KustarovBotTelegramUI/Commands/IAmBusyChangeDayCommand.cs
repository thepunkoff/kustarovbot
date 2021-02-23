using System;
using System.Net;
using System.Threading.Tasks;
using KustarovBotTelegramUI.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class IAmBusyChangeDayCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly string _day;
        private readonly bool _value;
        private readonly int _originalMessageId;
        public string DebugName { get; } = nameof(IAmBusyChangeDayCommand);

        public IAmBusyChangeDayCommand(TelegramBotClient botClient, ChatId chatId, string day, bool value, int originalMessageId = 0)
        {
            _botClient = botClient;
            _chatId = chatId;
            _day = day;
            _value = value;
            _originalMessageId = originalMessageId;
        }
        
        public async Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "iambusy/changeSchedule";
            ub.Query += $"?{_day}={_value}";

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
                    await new SendScheduleMenuCommand(_botClient, _chatId, _originalMessageId).Run();
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
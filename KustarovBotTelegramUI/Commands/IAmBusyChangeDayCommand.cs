using System;
using System.Net;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    // ReSharper disable once InconsistentNaming
    public class IAmBusyChangeDayCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string ChangeDay = "changeday";

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
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "iambusy/changeSchedule";
            ub.Query += $"?{_day}={_value}";

            Logger.Trace($"[{ChangeDay}] sending request to {ub}");
            var request = WebRequest.CreateHttp(ub.ToString());
            request.Method = "GET";

            try
            {
                var webResponse = await request.GetResponseAsync();
                var httpResponse = (HttpWebResponse) webResponse;
                Logger.Trace($"[{ChangeDay}] bot returned status code '{httpResponse.StatusCode}'");
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                    await new SendScheduleMenuCommand(_botClient, _chatId, _originalMessageId).Run();
            }
            catch (WebException wex)
            {
                var errorResponse = (HttpWebResponse) wex.Response;
                if (errorResponse is null)
                    Logger.Error($"[{ChangeDay}] {nameof(errorResponse)} was null");
                else
                    Logger.Trace($"[{ChangeDay}] bot returned status code '{errorResponse.StatusCode}'");
                await _botClient.SendTextMessageAsync(_chatId, "Произошла ошибка.");
                throw;
            }
        }
    }
}
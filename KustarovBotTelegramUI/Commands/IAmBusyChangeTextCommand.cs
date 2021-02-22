using System;
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
        private readonly Message _message;
        private readonly string _text;
        private bool _noOp;
        public string DebugName { get; }

        public IAmBusyChangeTextCommand(TelegramBotClient botClient, Message message, string[] args)
        {
            DebugName = nameof(IAmBusyChangeTextCommand);
            _botClient = botClient;
            _message = message;
            if (args.Length == 0)
            {
                Console.WriteLine("set target command args were empty. command will be no-op");
                _noOp = true;
                return;
            }

            _text = args[0];
        }
        
        public async Task Run()
        {
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += $"iambusy/changeText&text={_text}";

            Console.WriteLine($"sending request to {ub}");
            var request = WebRequest.CreateHttp(ub.ToString());
            request.Method = "GET";

            using var response = await request.GetResponseAsync();
            var httpResponse = (HttpWebResponse) response;

            Console.WriteLine($"bot returned status code'{httpResponse.StatusCode}'");

            if (httpResponse.StatusCode == HttpStatusCode.OK)
                await _botClient.SendTextMessageAsync(_message.Chat.Id, "Текст ответа изменен!");
            else
                await _botClient.SendTextMessageAsync(_message.Chat.Id, "Произошла ошибка.");
        }
    }
}
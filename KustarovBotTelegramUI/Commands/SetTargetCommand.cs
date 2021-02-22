using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KustarovBotTelegramUI.Commands
{
    public class SetTargetCommand : ICommand
    {
        private readonly TelegramBotClient _botClient;
        private readonly Message _message;
        private readonly string _uriString;

        private readonly bool _noOp;

        public string DebugName { get; init; }

        public SetTargetCommand(TelegramBotClient botClient, Message message, string[] args)
        {
            _botClient = botClient;
            _message = message;

            if (args.Length == 0)
            {
                Console.WriteLine("set target command args were empty. command will be no-op");
                _noOp = true;
                return;
            }

            DebugName = nameof(SetTargetCommand);
            _uriString = args[0];
        }


        public async Task Run()
        {
            if (_noOp)
                return;

            TelegramKustarovBotUI.Target = new Uri(_uriString);
            await _botClient.SendTextMessageAsync(_message.Chat.Id, $"Консоль теперь направлена на бота по адресу '{_uriString}'");
        }
    }
}
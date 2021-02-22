using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KustarovBotTelegramUI.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace KustarovBotTelegramUI
{
    public class TelegramKustarovBotUI
    {
        private TelegramBotClient _botClient;
        private CommandParser _commandParser;
        public static Uri Target = new("http://localhost:8080");
        
        public async Task Run()
        {
            var token = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt"));
            _botClient = new TelegramBotClient(token);
            _commandParser = new CommandParser(_botClient);

            _botClient.OnMessage += async (_, args) =>
            {
                try
                {
                    Console.WriteLine("new message");
                    var command = _commandParser.ParseCommand(args.Message);
                    Console.WriteLine($"running '{command.DebugName}' command");
                    await command.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error occured while processing user message:\n{ex}");
                    await new SendMessageCommand(_botClient, args.Message, "Произошла ошибка.").Run();
                }
            };
            
            _botClient.StartReceiving();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
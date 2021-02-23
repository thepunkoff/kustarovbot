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
        public static ProcedureCode ActiveProcedure = ProcedureCode.NoProcedure;
        
        public async Task Run()
        {
            var token = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt"));
            _botClient = new TelegramBotClient(token);
            _commandParser = new CommandParser(_botClient);

            _botClient.OnMessage += async (_, args) =>
            {
                try
                {
                    // ToDo: use "permittedUsers" file
                    if (args.Message.From.Id != 583334704 || args.Message.From.Id != 265677946)
                        await new SendRawMessageCommand(_botClient, args.Message.Chat.Id, "Нет доступа.").Run();
                    
                    Console.WriteLine($"message recieved:\n{args.Message.From.FirstName} {args.Message.From.LastName}: '{args.Message.Text}'");

                    ICommand command;
                    if (ActiveProcedure == ProcedureCode.IAmBusy_ChangeText)
                    {
                        command = new IAmBusyChangeTextCommand(_botClient, args.Message.Chat.Id, args.Message.Text);
                        ActiveProcedure = ProcedureCode.NoProcedure;
                    }
                    else
                    {
                        command = _commandParser.ParseCommand(args.Message.Chat.Id, args.Message.Text);
                    }

                    await command.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error occured while processing user message:\n{ex}");
                    await new SendRawMessageCommand(_botClient, args.Message.Chat.Id, "Произошла ошибка.").Run();
                }
            };
            
            _botClient.OnCallbackQuery += async (_, args) =>
            {
                try
                {
                    Console.WriteLine("callback query recieved");
                    foreach (var rawCommand in args.CallbackQuery.Data.Split(';'))
                    {
                        var command = _commandParser.ParseCommand(args.CallbackQuery.Message.Chat.Id, rawCommand, args.CallbackQuery.Message.MessageId);
                        await command.Run();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error occured while processing user message:\n{ex}");
                    await new SendRawMessageCommand(_botClient, args.CallbackQuery.Message.Chat.Id, "Произошла ошибка.").Run();
                }
            };
            
            _botClient.StartReceiving();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
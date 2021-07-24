using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KustarovBot.Shared;
using KustarovBotTelegramUI.Commands;
using NLog;
using Telegram.Bot;
using File = System.IO.File;

namespace KustarovBotTelegramUI
{
    public class TelegramKustarovBotUI
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Event = "event";

        private TelegramBotClient _botClient;
        private CommandParser _commandParser;
        public static Uri Target;
        public static ProcedureCode ActiveProcedure = ProcedureCode.NoProcedure;
        private readonly List<int> _permittedUsers = new();
        private readonly MailService _mailService;

        public TelegramKustarovBotUI(MailService mailService)
        {
            _mailService = mailService;
            Target = new Uri(Configuration.GetValue("botAddress").GetAwaiter().GetResult()); 
            var permittedUsers = Configuration.GetValues("permittedUsers").GetAwaiter().GetResult()
                .Select(int.Parse);
            _permittedUsers.AddRange(permittedUsers);
        }
        
        public async Task Run()
        {
            Logger.Info($"[{Event}] initializing KustarovBotTelegramUi v {Assembly.GetExecutingAssembly().GetName().Version}");
            var token = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "Configuration", "token.txt"));
            _botClient = new TelegramBotClient(token);
            _commandParser = new CommandParser(_botClient);

            _botClient.OnMessage += async (_, args) =>
            {
                try
                {
                    if (!_permittedUsers.Contains(args.Message.From.Id))
                    {
                        await new SendRawMessageCommand(_botClient, args.Message.Chat.Id, "Нет доступа.").Run();
                        return;
                    }
                    
                    Logger.Trace($"[{Event}] message recieved:\n{args.Message.From.FirstName} {args.Message.From.LastName}: '{args.Message.Text}'");

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

                    Logger.Trace($"Running {command.DebugName} command");
                    await command.Run();
                }
                catch (Exception ex)
                {
                    Logger.Error($"[{Event}] unexpected error occured while processing user message:\n{ex}");
                    await new SendRawMessageCommand(_botClient, args.Message.Chat.Id, "Произошла ошибка.").Run();
                    await _mailService.SendException(ex);
                }
            };
            
            _botClient.OnCallbackQuery += async (_, args) =>
            {
                try
                {
                    Logger.Trace($"[{Event}] callback query recieved:\n{args.CallbackQuery.From.FirstName} {args.CallbackQuery.From.LastName}: '{args.CallbackQuery.Data}'");
                    foreach (var rawCommand in args.CallbackQuery.Data.Split(';'))
                    {
                        var command = _commandParser.ParseCommand(args.CallbackQuery.Message.Chat.Id, rawCommand, args.CallbackQuery.Message.MessageId);
                        await command.Run();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[{Event}] unexpected error occured while processing user message:\n{ex}");
                    await new SendRawMessageCommand(_botClient, args.CallbackQuery.Message.Chat.Id, "Произошла ошибка.").Run();
                    await _mailService.SendException(ex);
                }
            };
            
            _botClient.StartReceiving();

            Logger.Info($"[{Event}] KustarovBotTelegramUi started.");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
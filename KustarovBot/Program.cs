using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KustarovBot.Http;
using KustarovBot.Modules;
using NLog;
using NLog.Config;
using NLog.Targets;
using VkNet;
using VkNet.Categories;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace KustarovBot
{
    public static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Start = "start";

        private static readonly VkApi VkApi = new();
        private static readonly List<IModule> MessageProcessors = new();
        private static EventProcessor _eventProcessor;
        private static readonly HttpServer HttpServer = new(8080);
        private static User _self;

        private static async Task Main()
        {
            try
            {
                Bootstrap();
                
                Logger.Info($"[{Start}] initializing KustarovBot v {Assembly.GetExecutingAssembly().GetName().Version}");
                await Authorize();
                AddMessageProcessors();
                HttpServer.Start();
                
                _eventProcessor = new EventProcessor(VkApi, _self);

                _eventProcessor.OnNewMessage += async (message, user) =>
                {
                    try
                    {
                        foreach (var processor in MessageProcessors)
                            await processor.ProcessMessage(message, user);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[{Start}] unexpected error occured while processing message:\n{ex}");
                    }
                };
                _eventProcessor.StartProcessingEvents();

                Logger.Info($"[{Start}] bot initialized successfully.");

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Logger.Error($"[{Start}] unhandled exception:\n{ex}");
            }
            finally
            {
                await _eventProcessor.DisposeAsync();
                await HttpServer.DisposeAsync();
                VkApi.Dispose();
            }
        }

        private static void Bootstrap()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget()
            {
                Name = "console",
                Layout = "${time} ${pad:padding=5:inner=${level:uppercase=true}} ${pad:padding=6:fixedLength=true:${activityid}} ${message}",
            };
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Trace", ForegroundColor = ConsoleOutputColor.Gray });
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Debug", ForegroundColor = ConsoleOutputColor.White });
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Info", ForegroundColor = ConsoleOutputColor.Green });
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Warn", ForegroundColor = ConsoleOutputColor.Yellow });
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Error", ForegroundColor = ConsoleOutputColor.Red });
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Fatal", ForegroundColor = ConsoleOutputColor.DarkRed });

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget, "*");
            foreach (var rule in config.LoggingRules)
                rule.EnableLoggingForLevel(LogLevel.Trace);

            LogManager.Configuration = config;
        }

        private static async Task Authorize()
        {
            await VkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt")),
                Settings = Settings.All | Settings.Offline,
            });


            var res = await VkApi.Users.GetAsync(Array.Empty<long>(), ProfileFields.Domain);
            _self = res.Single();
            Logger.Info($"[{Start}] authorized as {_self.FirstName} {_self.LastName} ({_self.Domain})");
        }

        private static void AddMessageProcessors()
        {
            Logger.Trace($"[{Start}] adding '{nameof(IAmBusyModule)}' module...");
            MessageProcessors.Add(new IAmBusyModule(VkApi, _self.Id));
            Logger.Info($"[{Start}] module '{nameof(IAmBusyModule)}' added.");
        }
    }
}

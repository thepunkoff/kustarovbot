using System;
using System.Threading.Tasks;
using KustarovBot.Shared;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace KustarovBotTelegramUI
{
    public static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Application = "application";

        private static readonly MailService MailService = new();

        private static async Task Main()
        {
            try
            {
                Bootstrap();
                var ui = new TelegramKustarovBotUI(MailService);
                await ui.Run();
            }
            catch (Exception ex)
            {
                Logger.Trace($"[{Application}] unhandled exception:\n{ex}");
                await MailService.SendException(ex);
            }
        }

        private static void Bootstrap()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget
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
    }
}
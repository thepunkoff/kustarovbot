using System;
using System.Threading.Tasks;
using NLog;

namespace KustarovBotTelegramUI
{
    public static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Application = "application";

        private static async Task Main()
        {
            try
            {
                var ui = new TelegramKustarovBotUI();
                await ui.Run();
            }
            catch (Exception ex)
            {
                Logger.Trace($"[{Application}] unhandled exception:\n{ex}");
            }
        }
    }
}
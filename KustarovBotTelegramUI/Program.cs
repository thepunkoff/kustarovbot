using System;
using System.Threading.Tasks;

namespace KustarovBotTelegramUI
{
    public static class Program
    {
        private static async Task Main()
        {
            try
            {
                var ui = new TelegramKustarovBotUI();
                await ui.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex}");
            }
        }
    }
}
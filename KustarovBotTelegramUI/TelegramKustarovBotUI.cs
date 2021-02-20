using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;

namespace KustarovBotTelegramUI
{
    public class TelegramKustarovBotUI
    {
        private TelegramBotClient _botClient;

        public async Task Run()
        {
            var token = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt"));
            _botClient = new TelegramBotClient(token);
            _botClient.OnMessage += async (_, args) =>
            {
                Console.WriteLine("new message");

                if (args.Message.Text != "/status")
                    return;

                Console.WriteLine("sending request to localhost:8080");
                var request = WebRequest.Create("http://localhost:8080");
                request.Method = "GET";

                using var response = await request.GetResponseAsync();
                await using var responseStream = response.GetResponseStream();
                var streamReader = new StreamReader(responseStream);
                var status = await streamReader.ReadToEndAsync();

                Console.WriteLine($"Bot returned status '{status}'. Sending to client");
                    
                await _botClient.SendTextMessageAsync(args.Message.Chat.Id, status);
            };
            
            _botClient.StartReceiving();

            while (true)
            {
            }
        }
    }
}
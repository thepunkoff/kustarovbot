using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;

namespace KustarovBot
{
    internal sealed class Program
    {
        static async Task Main()
        {
            var rng = new Random();
            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "token.txt")),
                Settings = Settings.All | Settings.Offline,
            });

            var processor = new EventProcessor(vkApi);
            var messageCounter = new UserMessageCounter();

            processor.OnNewMessage += async (message, user) =>
            {
                if (messageCounter.GetCount(user) % 10 == 0)
                {
                    try
                    {
                        await vkApi.Messages.SendAsync(new()
                        {
                            PeerId = user.Id,
                            Message = "Доброе время суток! Сегодня я не работаю, напишите мне в рабочий день. Спасибо за понимание!",
                            RandomId = rng.Next(),
                        });
                    }
                    catch (CaptchaNeededException)
                    {
                        Console.WriteLine("Captcha needed. Skipping.");
                    }
                }
                
                messageCounter.Increment(user);
            };
            processor.StartProcessingEvents();

            Console.ReadLine();
        }
    }
}

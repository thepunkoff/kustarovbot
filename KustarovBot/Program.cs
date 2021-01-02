using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;

namespace KustarovBot
{
    internal sealed class Program
    {
        private static readonly Random _rng = new();
        private static readonly VkApi _vkApi = new();
        private static readonly UserMessageCounter _messageCounter = new();
        private static readonly EventProcessor _eventProcessor;

        static Program()
        {
            _eventProcessor = new(_vkApi);
        }

        static async Task Main()
        {
            Console.WriteLine($"Initializing KustarovBot v {Assembly.GetExecutingAssembly().GetName().Version}");
            await _vkApi.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "token.txt")),
                Settings = Settings.All | Settings.Offline,
            });

            long authUserId = _vkApi.UserId ?? throw new Exception("VkApi.UserId was null. Should authorize using VkApi.Authorize");
            var res = await _vkApi.Users.GetAsync(Array.Empty<long>(), ProfileFields.Domain);
            var self = res.Single();
            Console.WriteLine($"Authorized as {self.FirstName} {self.LastName} ({self.Domain})");

            _eventProcessor.OnNewMessage += async (message, user) =>
            {
                if (_messageCounter.GetCount(user) % 10 == 0)
                {
                    try
                    {
                        await _vkApi.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                        {
                            PeerId = user.Id,
                            Message = "Доброе время суток! Сегодня я не работаю, напишите мне в рабочий день. Спасибо за понимание!!",
                            RandomId = _rng.Next(),
                        });
                    }
                    catch (CaptchaNeededException)
                    {
                        Console.WriteLine("Captcha needed. Skipping.");
                    }
                }
                
                _messageCounter.Increment(user);
            };
            await _eventProcessor.ProcessEvents();

            Console.WriteLine("Exiting with OK.");
        }
    }
}

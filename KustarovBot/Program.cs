using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private static EventProcessor _eventProcessor;

        static async Task Main()
        {
            Console.WriteLine($"Initializing KustarovBot v {Assembly.GetExecutingAssembly().GetName().Version}");
            await _vkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt")),
                Settings = Settings.All | Settings.Offline,
            });
            
            _eventProcessor = new EventProcessor(_vkApi);

            var authUserId = _vkApi.UserId ?? throw new Exception("VkApi.UserId was null. Should authorize using VkApi.Authorize");
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
                            Message = "Доброе время суток! Сегодня я не работаю, напишите мне в рабочий день. Спасибо за понимание!",
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

            Task.Run(() => ExposeHttp());
            await _eventProcessor.ProcessEvents();

            Console.WriteLine("Exiting with OK.");
        }

        private static HttpListener _httpListener;
        
        private static async Task ExposeHttp()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            Console.WriteLine("Started listening http requests on port 8080");

            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                
                Console.WriteLine($"got request from {request.RemoteEndPoint}");
                
                if (request.HttpMethod != "GET")
                {
                    Console.WriteLine("Responding only to GET requests");
                    continue;
                }
                
                var response = context.Response;
                const string responseString = "ok";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                await output.WriteAsync(buffer.AsMemory(0, buffer.Length));
                output.Close();

                Console.WriteLine("responded ok");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KustarovBot.Http;
using KustarovBot.MessageProcessing;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace KustarovBot
{
    public static class Program
    {
        private static readonly VkApi VkApi = new();
        private static readonly List<IModule> MessageProcessors = new();
        private static EventProcessor _eventProcessor;
        private static readonly HttpServer HttpServer = new(8080);

        private static async Task Main()
        {
            try
            {
                await Authorize();
                AddMessageProcessors();
                HttpServer.Start();

                _eventProcessor.OnNewMessage += async (message, user) =>
                {
                    try
                    {
                        foreach (var processor in MessageProcessors)
                            await processor.ProcessMessage(message, user);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"unexpected error occured while processing message:\n{ex}");
                    }
                };
                _eventProcessor.StartProcessingEvents();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"unhandled exception:\n{ex}");
            }
            finally
            {
                await _eventProcessor.DisposeAsync();
                await HttpServer.DisposeAsync();
                VkApi.Dispose();
            }
        }

        private static async Task Authorize()
        {
            Console.WriteLine($"Initializing KustarovBot v {Assembly.GetExecutingAssembly().GetName().Version}");
            await VkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "token.txt")),
                Settings = Settings.All | Settings.Offline,
            });

            _eventProcessor = new EventProcessor(VkApi);

            var res = await VkApi.Users.GetAsync(Array.Empty<long>(), ProfileFields.Domain);
            var self = res.Single();
            Console.WriteLine($"Authorized as {self.FirstName} {self.LastName} ({self.Domain})");
        }

        private static void AddMessageProcessors()
        {
            MessageProcessors.Add(new IAmBusyModule(VkApi));
            Console.WriteLine($"Added {nameof(IAmBusyModule)}");
        }
    }
}

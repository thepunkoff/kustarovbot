using System;
using System.Threading.Tasks;
using KustarovBot.Utils;
using VkNet;
using VkNet.Exception;
using VkNet.Model;

namespace KustarovBot.MessageProcessing
{
    // ReSharper disable once InconsistentNaming
    public class IAmBusyModule : IModule
    {
        private readonly VkApi _vkApi;
        private readonly Random _rng = new();
        private readonly UserMessageCounter _messageCounter = new();

        public static string ResponseText = "Доброе время суток! Сегодня я не работаю, напишите мне в рабочий день. Спасибо за понимание!";

        public IAmBusyModule(VkApi vkApi)
        {
            _vkApi = vkApi;
        }
        
        public async Task ProcessMessage(Message message, User user)
        {
            if (_messageCounter.GetCount(user) % 10 == 0)
            {
                try
                {
                    await _vkApi.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        PeerId = user.Id,
                        Message = ResponseText,
                        RandomId = _rng.Next(),
                    });
                }
                catch (CaptchaNeededException)
                {
                    Console.WriteLine("Captcha needed. Skipping.");
                }
            }
                
            _messageCounter.Increment(user);
        }
    }
}
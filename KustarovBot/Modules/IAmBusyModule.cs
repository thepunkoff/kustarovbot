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
                    var dayOfWeek = DateTime.Now.DayOfWeek;
                    var state = Resources.LoadState();
                    var mondayOk = dayOfWeek == DayOfWeek.Monday && state.Monday;
                    var tuesdayOk = dayOfWeek == DayOfWeek.Tuesday && state.Tuesday;
                    var wednesdayOk = dayOfWeek == DayOfWeek.Wednesday && state.Wednesday;
                    var thursdayOk = dayOfWeek == DayOfWeek.Thursday && state.Thursday;
                    var fridayOk = dayOfWeek == DayOfWeek.Friday && state.Friday;
                    var saturdayOk = dayOfWeek == DayOfWeek.Saturday && state.Saturday;
                    var sundayOk = dayOfWeek == DayOfWeek.Sunday && state.Sunday;

                    if (mondayOk || tuesdayOk || wednesdayOk || thursdayOk || fridayOk || saturdayOk || sundayOk)
                    {
                        await _vkApi.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams
                        {
                            PeerId = user.Id,
                            Message = state.ReplyText,
                            RandomId = _rng.Next(),
                        });
                    }
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
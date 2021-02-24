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
        private const int SkipCount = 10;

        public IAmBusyModule(VkApi vkApi)
        {
            _vkApi = vkApi;
        }
        
        public async Task ProcessMessage(Message message, User user)
        {
            Console.WriteLine("[iambusy] processing message...");
            var dayOfWeek = DateTime.Now.DayOfWeek;
            var state = Resources.LoadState();
            var mondayOk = dayOfWeek == DayOfWeek.Monday && state.Monday;
            var tuesdayOk = dayOfWeek == DayOfWeek.Tuesday && state.Tuesday;
            var wednesdayOk = dayOfWeek == DayOfWeek.Wednesday && state.Wednesday;
            var thursdayOk = dayOfWeek == DayOfWeek.Thursday && state.Thursday;
            var fridayOk = dayOfWeek == DayOfWeek.Friday && state.Friday;
            var saturdayOk = dayOfWeek == DayOfWeek.Saturday && state.Saturday;
            var sundayOk = dayOfWeek == DayOfWeek.Sunday && state.Sunday;

            if (!mondayOk && !tuesdayOk && !wednesdayOk && !thursdayOk && !fridayOk && !saturdayOk && !sundayOk)
            {
                Console.WriteLine($"[iambusy] day of week '{dayOfWeek}' is ignored in bot rules. message won't be processed.");
                return;
            }

            var countForUser = _messageCounter.GetCount(user);
            var modulo = countForUser % SkipCount;
            if (modulo == 0)
            {
                try
                {
                    Console.WriteLine("[iambusy] sending reply text.");
                    await _vkApi.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams
                    {
                        PeerId = user.Id,
                        Message = state.ReplyText,
                        RandomId = _rng.Next(),
                    });
                }
                catch (CaptchaNeededException)
                {
                    Console.WriteLine("captcha needed. skipping.");
                }
            }
            else
            {
                Console.WriteLine($"[iambusy] skipping message ({modulo}/{SkipCount})");
            }
                
            _messageCounter.Increment(user);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KustarovBot.Utils;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace KustarovBot.Modules
{
    // ReSharper disable once InconsistentNaming
    public class IAmBusyModule : IModule
    {
        private readonly VkApi _vkApi;
        private readonly Random _rng = new();
        private readonly UserMessageCounter _messageCounter = new();
        private readonly long[] _ignoredUserIds;
        private const int SkipCount = 10;

        public IAmBusyModule(VkApi vkApi, long selfId)
        {
            _vkApi = vkApi;
            var friendLists = _vkApi.Friends.GetLists(selfId);
            if (friendLists.TotalCount == 0)
                Console.WriteLine("[iambusy] no friend lists were found. all messages will be processed by the module.");
            
            var state = Resources.LoadState();
            var ignoreList = friendLists.SingleOrDefault(x => x.Name == state.BotIgnoreListName);
            if (ignoreList is null)
            {
                Console.WriteLine($"[iambusy] ignore list with name '{state.BotIgnoreListName}' doesn't exist. all messages will be processed by the module.");
                return;
            }

            Console.WriteLine($"[iambusy] filtering by ignore list '{state.BotIgnoreListName}' is on.");

            _ignoredUserIds = _vkApi.Friends.Get(new FriendsGetParams()
            {
                ListId = ignoreList.Id,
                Fields = ProfileFields.Uid
            }).Select(x => x.Id).ToArray();
            
            Console.WriteLine($"[iambusy] {_ignoredUserIds.Length} ignored users found.");
        }
        
        public async Task ProcessMessage(Message message, User user)
        {
            Console.WriteLine("[iambusy] processing message...");

            if (_ignoredUserIds.Contains(user.Id))
            {
                Console.WriteLine($"[iambusy] user '{user.FirstName} {user.LastName} ({user.Domain})' was found in the ignore list. message won't be processed.");
                return;
            }
            
            Console.WriteLine($"[iambusy] user '{user.FirstName} {user.LastName} ({user.Domain})' is not in the ignore list.");
            
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
                    await _vkApi.Messages.SendAsync(new MessagesSendParams
                    {
                        PeerId = user.Id,
                        Message = state.ReplyText,
                        RandomId = _rng.Next(),
                        Payload = "{ \"bot\": \"true\" }"
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
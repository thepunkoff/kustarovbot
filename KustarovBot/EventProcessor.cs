using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace KustarovBot
{
    public class EventProcessor
    {
        private readonly VkApi _vk;
        private readonly ulong _ts;
        private ulong? _pts;

        public event Action<Message, User> OnNewMessage;
        
        public EventProcessor(VkApi api)
        {
            _vk = api;
            var longPoolServerResponse = _vk.Messages.GetLongPollServer(needPts: true);
            _ts = Convert.ToUInt64(longPoolServerResponse.Ts);
            _pts = longPoolServerResponse.Pts;
        }

        public async Task ProcessEvents()
        {
            try
            {
                while (true)
                {
                    var longPollResponse = await _vk.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams()
                    {
                        Ts = _ts,
                        Pts = _pts,
                        Fields = UsersFields.Domain,
                    });

                    _pts = longPollResponse.NewPts;

                    foreach (var history in longPollResponse.History)
                    {
                        var eventCode = history[0];
                        switch (eventCode)
                        {
                            case 4:
                                var history1 = history;
                                var message = longPollResponse.Messages.SingleOrDefault(x => x.Id == history1[1]);
                                var user = longPollResponse.Profiles.SingleOrDefault(x => x.Id == message.FromId);
                                OnNewMessage?.Invoke(message, user);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(ProcessEvents)} crashed:\n{ex}");
            }
        }
    }
}

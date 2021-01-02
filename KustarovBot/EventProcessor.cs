using System;
using System.Collections.Generic;
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
        private readonly CancellationTokenSource _cts = new();

        private ulong _timeStamp;
        private ulong? _pts;

        public event Action<Message, User> OnNewMessage;
        public EventProcessor(VkApi api)
        {
            _vk = api;
        }

        public void StartProcessingEvents()
        {
            var longPoolServerResponse = _vk.Messages.GetLongPollServer(needPts: true);
            _timeStamp = Convert.ToUInt64(longPoolServerResponse.Ts);
            _pts = longPoolServerResponse.Pts;
            var handlingWorker = Task.Run(() => LongPollWorker(_cts.Token), _cts.Token);
        }

        public async Task LongPollWorker(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var longPollResponse = await _vk.Messages.GetLongPollHistoryAsync(new()
                    {
                        Ts = _timeStamp,
                        Pts = _pts,
                        Fields = UsersFields.Domain,
                    });

                    _pts = longPollResponse.NewPts;

                    for (int i = 0; i <  longPollResponse.History.Count; i++)
                    {
                        var history = longPollResponse.History[i];
                        var eventCode = history[0];

                        switch (eventCode)
                        {
                            case 4:
                                var message = longPollResponse.Messages.Where(x => x.Id == history[1]).SingleOrDefault();
                                var user = longPollResponse.Profiles.Where(x => x.Id == message.FromId).SingleOrDefault();
                                OnNewMessage?.Invoke(message, user);
                                break;
                        }
                    }
                }

                Console.WriteLine($"{nameof(LongPollWorker)} cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(LongPollWorker)} crashed:\n{ex}");
            }
        }
    }
}

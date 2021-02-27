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
    public class EventProcessor : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        
        private readonly VkApi _vk;
        private readonly User _self;

        private readonly ulong _ts;
        private ulong? _pts;

        private Task _eventProcessingWorker;

        public event Action<Message, User> OnNewMessage;
        
        public EventProcessor(VkApi api, User self)
        {
            _vk = api;
            _self = self;
            var longPoolServerResponse = _vk.Messages.GetLongPollServer(needPts: true);
            _ts = Convert.ToUInt64(longPoolServerResponse.Ts);
            _pts = longPoolServerResponse.Pts;
        }

        public void StartProcessingEvents()
        {
            _eventProcessingWorker = Task.Run(EventProcessingWorker, _cts.Token);
        }

        private async Task EventProcessingWorker()
        {
            try
            {
                while (true)
                {
                    var longPollResponse = await _vk.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams
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
                            {
                                var message = longPollResponse.Messages.SingleOrDefault(x => x.Id == history[1]);
                                if (message is null)
                                {
                                    Console.WriteLine("[event] ERROR: message was null!");
                                    break;
                                }

                                // Игнорируем свои сообщения кому-либо
                                // Todo: понять, как игнорировать сообщения бота
                                if (message.FromId == _self.Id && message.FromId != message.PeerId)
                                {
                                    Console.WriteLine("[event] ignoring bot message.");
                                    break;
                                }

                                if (message.FromId != message.PeerId)
                                    Console.WriteLine("[event] can't ignore self message yet.");

                                var user = longPollResponse.Profiles.SingleOrDefault(x => x.Id == message.FromId);
                                if (user is null)
                                {
                                    Console.WriteLine("[event] ERROR: user was null!");
                                    break;
                                }
                                
                                Console.WriteLine($"[event] incoming message from user {user.FirstName} {user.LastName} ({user.Domain})");
                                OnNewMessage?.Invoke(message, user);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(EventProcessingWorker)} crashed:\n{ex}");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _eventProcessingWorker;
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}

﻿using System.Collections.Generic;
using VkNet.Model;

namespace KustarovBot.Utils
{
    public sealed class UserMessageCounter
    {
        private readonly Dictionary<long, int> _counters = new();

        public void Increment(User user)
        {
            if (!_counters.ContainsKey(user.Id))
                _counters.Add(user.Id, 0);
                
            _counters[user.Id]++;
        }

        public int GetCount(User user)
        {
            return _counters.ContainsKey(user.Id)
                ? _counters[user.Id]
                : 0;
        }
    }
}

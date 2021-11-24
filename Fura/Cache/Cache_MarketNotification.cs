using System;
using System.Collections.Generic;
using Neo.Plugins.Models;
using System.Numerics;
using MongoDB.Entities;
using System.Collections.Concurrent;
using Neo.Persistence;

namespace Neo.Plugins.Cache
{
    public class CacheMatketNotification : IDBCache
    {
        private ConcurrentBag<MarketNotificationModel> L_MarketNotificationModel;

        public CacheMatketNotification()
        {
            L_MarketNotificationModel = new ConcurrentBag<MarketNotificationModel>();
        }

        public void Clear()
        {
            L_MarketNotificationModel = new ConcurrentBag<MarketNotificationModel>();
        }

        public void Add(UInt256 txid, UInt256 blockHash, UInt160 user, string eventName, string @params, ulong timestamp)
        {
            MarketNotificationModel marketNotificationModel = new(txid, blockHash, user, eventName, @params, timestamp);
            L_MarketNotificationModel.Add(marketNotificationModel);
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_MarketNotificationModel.Count > 0)
                tran.SaveAsync(L_MarketNotificationModel).Wait();
        }
    }
}

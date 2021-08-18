using System;
using System.Collections.Generic;
using Neo.Plugins.Models;
using System.Numerics;
using MongoDB.Entities;
using System.Collections.Concurrent;
using Neo.Persistence;

namespace Neo.Plugins.Cache
{
    public class CacheTransferNotification : IDBCache
    {
        private ConcurrentBag<TransferNotificationModel> L_TransferNotificationModel;

        public CacheTransferNotification()
        {
            L_TransferNotificationModel = new ConcurrentBag<TransferNotificationModel>();
        }

        public void Clear()
        {
            L_TransferNotificationModel = new ConcurrentBag<TransferNotificationModel>();
        }

        public void Add(UInt256 txid, UInt256 blockHash, ulong timestamp, UInt160 contractHash, UInt160 from, UInt160 to, BigInteger value, BigInteger fromBalance, BigInteger toBalance)
        {
            TransferNotificationModel transferNotificationModel = new(txid, blockHash, timestamp, contractHash, from, to, value, fromBalance, toBalance);
            L_TransferNotificationModel.Add(transferNotificationModel);
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_TransferNotificationModel.Count > 0)
                tran.SaveAsync(L_TransferNotificationModel).Wait();
        }
    }
}

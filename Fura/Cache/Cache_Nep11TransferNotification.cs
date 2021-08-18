using System;
using System.Collections.Generic;
using Neo.Plugins.Models;
using System.Numerics;
using MongoDB.Entities;
using System.Collections.Concurrent;
using Neo.Persistence;

namespace Neo.Plugins.Cache
{
    public class CacheNep11TransferNotification : IDBCache
    {
        private ConcurrentBag<Nep11TransferNotificationModel> L_Nep11TransferNotificationModel;

        public CacheNep11TransferNotification()
        {
            L_Nep11TransferNotificationModel = new ConcurrentBag<Nep11TransferNotificationModel>();
        }

        public void Clear()
        {
            L_Nep11TransferNotificationModel = new ConcurrentBag<Nep11TransferNotificationModel>();
        }

        public void Add(UInt256 txid, UInt256 blockHash, ulong timestamp, UInt160 contractHash, string tokenid, UInt160 from, UInt160 to, BigInteger value, BigInteger fromBalance, BigInteger toBalance)
        {
            Nep11TransferNotificationModel nep11TransferNotificationModel = new(txid, blockHash, timestamp, contractHash, tokenid, from, to, value, fromBalance, toBalance);
            L_Nep11TransferNotificationModel.Add(nep11TransferNotificationModel);
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_Nep11TransferNotificationModel.Count > 0)
                tran.SaveAsync(L_Nep11TransferNotificationModel).Wait();
        }
    }
}

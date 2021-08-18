using System;
using System.Collections.Generic;
using System.Numerics;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using Neo.SmartContract;
using System.Linq;

namespace Neo.Plugins.Cache
{
    public class DBCache
    {
        private static DBCache ins;

        private static object lockObj = new object();

        public static DBCache Ins
        {
            get
            {
                if (ins is null)
                {
                    lock (lockObj)
                    {
                        if (ins is null)
                            ins = new DBCache();
                    }
                }
                return ins;
            }

        }
        public CacheAddress cacheAddress { get; }
        public CacheAddressAsset cacheAddressAsset { get; }
        public CacheAsset cacheAsset { get; }
        public CacheCandidate cacheCandidate { get; }
        public CacheContract cacheContract { get; }
        public CacheNep11TransferNotification cacheNep11TransferNotification { get; }
        public CacheScVoteCall cacheScVoteCall { get; }
        public CacheTransferNotification cacheTransferNotification;
        public CacheVote cacheVote { get; }
        public CacheScCall cacheScCall { get; }
        public CacheExecution cacheExecution { get; }
        public CacheNotification cacheNotification { get; }
        public CacheGasMintBurn cacheGasMintBurn { get; }
        private List<IDBCache> caches;
        public DBCache()
        {
            caches = new List<IDBCache>();

            cacheAddress = new CacheAddress();
            caches.Add(cacheAddress);

            cacheAddressAsset = new CacheAddressAsset();
            caches.Add(cacheAddressAsset);

            cacheAsset = new CacheAsset();
            caches.Add(cacheAsset);

            cacheCandidate = new CacheCandidate();
            caches.Add(cacheCandidate);

            cacheContract = new CacheContract();
            caches.Add(cacheContract);

            cacheNep11TransferNotification = new CacheNep11TransferNotification();
            caches.Add(cacheNep11TransferNotification);

            cacheScVoteCall = new CacheScVoteCall();
            caches.Add(cacheScVoteCall);

            cacheTransferNotification = new CacheTransferNotification();
            caches.Add(cacheTransferNotification);

            cacheVote = new CacheVote();
            caches.Add(cacheVote);

            cacheScCall = new CacheScCall();
            caches.Add(cacheScCall);

            cacheExecution = new CacheExecution();
            caches.Add(cacheExecution);

            cacheNotification = new CacheNotification();
            caches.Add(cacheNotification);

            cacheGasMintBurn = new CacheGasMintBurn();
            caches.Add(cacheGasMintBurn);

        }

        public void Reset()
        {
            foreach(var c in caches)
            {
                c.Clear();
            }
        }

        public void Save(NeoSystem system, DataCache snapshot, Transaction transaction)
        {
            foreach (var c in caches)
            {
                c.Update(system, snapshot);
            }
            foreach (var c in caches)
            {
                c.Save(transaction);
            }
        }
    }
}

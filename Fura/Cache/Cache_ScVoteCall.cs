using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheScVoteCall : IDBCache
    {
        private ConcurrentBag<ScVoteCallModel> L_ScVoteCallModel;

        public CacheScVoteCall()
        {
            L_ScVoteCallModel = new ConcurrentBag<ScVoteCallModel>();
        }

        public void Clear()
        {
            L_ScVoteCallModel = new ConcurrentBag<ScVoteCallModel>();
        }

        public ScVoteCallModel Add(UInt256 txid, uint index, UInt160 voter, UInt160 candidate, string candidatePubKey)
        {
            var scVoteCallModel = new ScVoteCallModel(txid, index, voter, candidate, candidatePubKey);
            L_ScVoteCallModel.Add(scVoteCallModel);
            return scVoteCallModel;
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_ScVoteCallModel.Count > 0)
                tran.SaveAsync(L_ScVoteCallModel).Wait();
        }
    }
}

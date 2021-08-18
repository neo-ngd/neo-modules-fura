using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Entities;
using Neo.Plugins.Models;
using System.Linq;
using Neo.Persistence;

namespace Neo.Plugins.Cache
{
    public class CacheScCall : IDBCache
    {
        private ConcurrentBag<ScCallModel> L_ScCallModel;

        public CacheScCall()
        {
            L_ScCallModel = new ConcurrentBag<ScCallModel>();
        }


        public void Clear()
        {
            L_ScCallModel = new ConcurrentBag<ScCallModel>();
        }

        public void Add(List<ScCallModel> scCallModels)
        {
            foreach (var scCallModel in scCallModels)
            {
                L_ScCallModel.Add(scCallModel);
            }
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_ScCallModel.Count > 0)
                tran.SaveAsync(L_ScCallModel).Wait();
        }
    }
}

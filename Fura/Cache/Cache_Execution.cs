using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheExecution : IDBCache
    {
        private ConcurrentBag<ExecutionModel> L_ExecutionModel;

        public CacheExecution()
        {
            L_ExecutionModel = new ConcurrentBag<ExecutionModel>();
        }


        public void Clear()
        {
            L_ExecutionModel = new ConcurrentBag<ExecutionModel>();
        }

        public void Add(ExecutionModel executionModel)
        {
            L_ExecutionModel.Add(executionModel);
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_ExecutionModel.Count > 0)
                tran.SaveAsync(L_ExecutionModel).Wait();
        }
    }
}

using System;
using Neo.Persistence;

namespace Neo.Plugins.Cache
{
    public interface IDBCache
    {
        void Clear();
        void Save(MongoDB.Entities.Transaction tran);
        void Update(NeoSystem system, DataCache snapshot);
    }
}

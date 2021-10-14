using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheNep11PropertiesParams
    {
        public UInt160 Asset;
        public string Tokenid;
    }

    public class CacheNep11Properties : IDBCache
    {
        private ConcurrentDictionary<(UInt160, string), CacheNep11PropertiesParams> D_Nep11Properties;
        private ConcurrentDictionary<(UInt160, string), Nep11PropertiesModel> D_Nep11PropertiesModel;

        public CacheNep11Properties()
        {
            D_Nep11Properties = new ConcurrentDictionary<(UInt160, string), CacheNep11PropertiesParams>();
            D_Nep11PropertiesModel = new ConcurrentDictionary<(UInt160, string), Nep11PropertiesModel>();
        }

        public void Clear()
        {
            D_Nep11Properties = new ConcurrentDictionary<(UInt160, string), CacheNep11PropertiesParams>();
            D_Nep11PropertiesModel = new ConcurrentDictionary<(UInt160, string), Nep11PropertiesModel>();
        }

        public void AddNeedUpdate( UInt160 asset, string tokenid)
        {
            D_Nep11Properties[(asset, tokenid)] = new() {Asset = asset, Tokenid = tokenid };
        }

        public List<CacheNep11PropertiesParams> GetNeedUpdate()
        {
            return D_Nep11Properties.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheNep11PropertiesParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                //从vm中获取properties
                var properties = Neo.Plugins.VM.Helper.GetNep11Properties(system, snapshot, list[i].Asset, list[i].Tokenid);
                AddOrUpdate(list[i].Asset, list[i].Tokenid, properties);
            });
        }

        public Nep11PropertiesModel Get(UInt160 asset, string tokenid)
        {
            if (D_Nep11PropertiesModel.ContainsKey((asset, tokenid)))
            {
                return D_Nep11PropertiesModel[(asset, tokenid)];
            }
            else
            {
                return Nep11PropertiesModel.Get(asset, tokenid);
            }
        }

        public void AddOrUpdate(UInt160 asset, string tokenid, string properties)
        {
            Nep11PropertiesModel nep11PropertiesModel = Get(asset, tokenid);
            if (nep11PropertiesModel is null)
            {
                nep11PropertiesModel = new(asset, tokenid, properties);
            }
            else
            {
                nep11PropertiesModel.Properties = properties;
            }
            D_Nep11PropertiesModel[(asset, tokenid)] = nep11PropertiesModel;
        }

        public void Save(Transaction tran)
        {
            if (D_Nep11PropertiesModel.Values.Count > 0)
                tran.SaveAsync(D_Nep11PropertiesModel.Values).Wait();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Configuration.Hocon;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using System.Numerics;

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

        private ConcurrentDictionary<(UInt160, string), IlexPropertiesModel> D_IlexPropertiesModel;

        private ConcurrentDictionary<(UInt160, string), MetaPropertiesModel> D_MetaPropertiesModel;

        public CacheNep11Properties()
        {
            D_Nep11Properties = new ConcurrentDictionary<(UInt160, string), CacheNep11PropertiesParams>();
            D_Nep11PropertiesModel = new ConcurrentDictionary<(UInt160, string), Nep11PropertiesModel>();
            D_IlexPropertiesModel = new ConcurrentDictionary<(UInt160, string), IlexPropertiesModel>();
            D_MetaPropertiesModel = new ConcurrentDictionary<(UInt160, string), MetaPropertiesModel>();
        }

        public void Clear()
        {
            D_Nep11Properties = new ConcurrentDictionary<(UInt160, string), CacheNep11PropertiesParams>();
            D_Nep11PropertiesModel = new ConcurrentDictionary<(UInt160, string), Nep11PropertiesModel>();
            D_IlexPropertiesModel = new ConcurrentDictionary<(UInt160, string), IlexPropertiesModel>();
            D_MetaPropertiesModel = new ConcurrentDictionary<(UInt160, string), MetaPropertiesModel>();
        }

        public void AddNeedUpdate(UInt160 asset, string tokenid)
        {
            D_Nep11Properties[(asset, tokenid)] = new() { Asset = asset, Tokenid = tokenid };
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
                var isSelfControl = Neo.Plugins.VM.Helper.IsSelfControl(system, snapshot, list[i].Asset);
                AddOrUpdate(list[i].Asset, list[i].Tokenid, properties, isSelfControl);
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

        public IlexPropertiesModel GetIlex(UInt160 asset, string tokenid)
        {
            if (D_IlexPropertiesModel.ContainsKey((asset, tokenid)))
            {
                return D_IlexPropertiesModel[(asset, tokenid)];
            }
            else
            {
                return IlexPropertiesModel.Get(asset, tokenid);
            }
        }

        public MetaPropertiesModel GetMeta(UInt160 asset, string tokenid)
        {
            if (D_MetaPropertiesModel.ContainsKey((asset, tokenid)))
            {
                return D_MetaPropertiesModel[(asset, tokenid)];
            }
            else
            {
                return MetaPropertiesModel.Get(asset, tokenid);
            }
        }

        public void AddOrUpdate(UInt160 asset, string tokenid, string properties, BigInteger selfControl)
        {
            AddOrUpdateNep11(asset, tokenid, properties);
            if (Settings.Default.MetaContractHashes.Contains(asset.ToString()) || selfControl == 1)
            {
                AddOrUpdateIlex(asset, tokenid, properties);
            }
            if (Settings.Default.MetaContractHashes.Contains(asset.ToString()) || selfControl == 2)
            {
                AddOrUpdateMeta(asset, tokenid, properties);
            }
        }

        private void AddOrUpdateNep11(UInt160 asset, string tokenid, string properties)
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

        private void AddOrUpdateIlex(UInt160 asset, string tokenid, string properties)
        {
            IlexPropertiesModel ilexPropertiesModel = GetIlex(asset, tokenid);
            if (ilexPropertiesModel is null)
            {
                ilexPropertiesModel = new(asset, tokenid, properties);
            }
            else
            {
                ilexPropertiesModel.UpdateProperties(properties);
            }
            D_IlexPropertiesModel[(asset, tokenid)] = ilexPropertiesModel;
        }

        private void AddOrUpdateMeta(UInt160 asset, string tokenid, string properties)
        {
            MetaPropertiesModel metaPropertiesModel = GetMeta(asset, tokenid);
            if (metaPropertiesModel is null)
            {
                metaPropertiesModel = new(asset, tokenid, properties);
            }
            else
            {
                metaPropertiesModel.UpdateProperties(properties);
            }
            D_MetaPropertiesModel[(asset, tokenid)] = metaPropertiesModel;

        }

        public void Save(Transaction tran)
        {
            if (D_Nep11PropertiesModel.Values.Count > 0)
                tran.SaveAsync(D_Nep11PropertiesModel.Values).Wait();
            if (D_IlexPropertiesModel.Values.Count > 0)
                tran.SaveAsync(D_IlexPropertiesModel.Values).Wait();
            if (D_MetaPropertiesModel.Values.Count > 0)
                tran.SaveAsync(D_MetaPropertiesModel.Values).Wait();
        }
    }
}

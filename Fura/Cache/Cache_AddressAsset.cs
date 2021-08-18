using System.Collections.Generic;
using System.Numerics;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Plugins.Models;
using System.Linq;
using System.Threading.Tasks;
using Neo.Persistence;
using System.Collections.Concurrent;

namespace Neo.Plugins.Cache
{

    public class CacheAddressAssetParams
    {
        public UInt160 Address;
        public UInt160 Asset;
        public string Tokenid;
    }

    public class CacheAddressAsset : IDBCache
    {
        private ConcurrentDictionary<(UInt160, UInt160, string), CacheAddressAssetParams> D_AddressAsset;
        private ConcurrentDictionary<(UInt160, UInt160, string), AddressAssetModel> D_AddressAssetModel;

        public CacheAddressAsset()
        {
            D_AddressAssetModel = new ConcurrentDictionary<(UInt160, UInt160, string), AddressAssetModel>();
            D_AddressAsset = new ConcurrentDictionary<(UInt160, UInt160, string), CacheAddressAssetParams>();
        }

        public void Clear()
        {
            D_AddressAssetModel = new ConcurrentDictionary<(UInt160, UInt160, string), AddressAssetModel>();
            D_AddressAsset = new ConcurrentDictionary<(UInt160, UInt160, string), CacheAddressAssetParams>();
        }

        public void AddNeedUpdate(UInt160 address, UInt160 asset, string tokenid)
        {
            D_AddressAsset[(address, asset, tokenid)] = new() { Address = address, Asset = asset, Tokenid = tokenid };
        }

        public List<CacheAddressAssetParams> GetNeedUpdate()
        {
            return D_AddressAsset.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheAddressAssetParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                BigInteger balance;
                if(list[i].Tokenid is null || list[i].Tokenid  == string.Empty)
                {
                    balance = Neo.Plugins.VM.Helper.GetNep17BalanceOf(system, snapshot, list[i].Asset, list[i].Address)[0];
                }
                else
                {
                    balance = Neo.Plugins.VM.Helper.GetNep11BalanceOf(system, snapshot, list[i].Asset, list[i].Tokenid, list[i].Address)[0];
                }
                AddOrUpdate(list[i].Address, list[i].Asset, balance, list[i].Tokenid);
            });
        }

        public AddressAssetModel Get(UInt160 address, UInt160 asset, string tokenid)
        {
            if (D_AddressAssetModel.ContainsKey((address, asset, tokenid)))
            {
                return D_AddressAssetModel[(address, asset, tokenid)];
            }
            else
            {
                return AddressAssetModel.Get(address, asset, tokenid);
            }
        }

        public void AddOrUpdate(UInt160 address, UInt160 asset, BigInteger balance, string tokenid)
        {
            if (address == null || asset == null)
                return;
            AddressAssetModel addressAssetModel = Get(address, asset, tokenid);
            if (addressAssetModel is null)
            {
                addressAssetModel = new AddressAssetModel(address, asset, balance, tokenid);
            }
            else
            {
                addressAssetModel.Balance = BsonDecimal128.Create(balance.ToString());
            }
            D_AddressAssetModel[(address, asset, tokenid)] = addressAssetModel;
        }

        public void Save(Transaction tran)
        {
            if (D_AddressAssetModel.Values.Count > 0)
                tran.SaveAsync(D_AddressAssetModel.Values).Wait();
        }
    }
}

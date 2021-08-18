using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Plugins.Models;
using System.Linq;
using Neo.Persistence;
using System.Threading.Tasks;
using Neo.SmartContract;

namespace Neo.Plugins.Cache
{

    public class CacheAssetParams
    {
        public UInt160 Hash;
        public ulong Time;
        public UInt256 Txid;
    }

    public class CacheAsset : IDBCache
    {
        private ConcurrentDictionary<UInt160, CacheAssetParams> D_Asset;
        private ConcurrentDictionary<UInt160, AssetModel> D_AssetModel;

        public CacheAsset()
        {
            D_AssetModel = new ConcurrentDictionary<UInt160, AssetModel>();
            D_Asset = new ConcurrentDictionary<UInt160, CacheAssetParams>();
        }

        public void Clear()
        {
            D_AssetModel = new ConcurrentDictionary<UInt160, AssetModel>();
            D_Asset = new ConcurrentDictionary<UInt160, CacheAssetParams>();
        }

        public void AddNeedUpdate(UInt160 contractHash, ulong time, UInt256 txid)
        {
            D_Asset[contractHash] = new() { Hash = contractHash , Time = time, Txid = txid};
        }

        public List<CacheAssetParams> GetNeedUpdate()
        {
            return D_Asset.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheAssetParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                //获取asset的decimals totalsupply等信息
                var t = Neo.Plugins.VM.Helper.GetAssetInfo(system, snapshot, list[i].Hash);
                StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.ContractManagement.Id, 8).Add(list[i].Hash);
                ContractState contract = snapshot.TryGet(key)?.GetInteroperable<ContractState>();
                EnumAssetType type;
                if (contract.Manifest.SupportedStandards.Contains("NEP-17"))
                {
                    type = EnumAssetType.NEP17;
                }
                else if (contract.Manifest.SupportedStandards.Contains("NEP-11"))
                {
                    type = EnumAssetType.NEP11;
                }
                else
                {
                    type = EnumAssetType.Unknown;
                }
                AddOrUpdate(list[i].Hash, list[i].Time, contract.Manifest.Name, t.Item2, t.Item1, t.Item3, type);
            });
        }

        public AssetModel Get(UInt160 contractHash)
        {
            if (D_AssetModel.ContainsKey(contractHash))
            {
                return D_AssetModel[contractHash];
            }
            else
            {
                return AssetModel.Get(contractHash);
            }
        }

        public void AddOrUpdate(UInt160 contractHash, ulong firstTransferTime, string tokenName, byte decimals, string symbol, BigInteger totalSupply, EnumAssetType enumAssetType)
        {
            AssetModel assetModel = Get(contractHash);
            if (assetModel is null)
            {
                assetModel = new(contractHash, firstTransferTime, tokenName, decimals, symbol, totalSupply, enumAssetType);
            }
            else
            {
                assetModel.TokenName = tokenName;
                assetModel.Decimals = decimals;
                assetModel.Symbol = symbol;
                assetModel.TotalSupply = BsonDecimal128.Create(totalSupply.ToString());
            }
            D_AssetModel[contractHash] = assetModel;
        }

        public void Save(Transaction tran)
        {
            if (D_AssetModel.Values.Count > 0)
                tran.SaveAsync(D_AssetModel.Values).Wait();
        }
    }
}

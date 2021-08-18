using System.Collections.Generic;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using Neo.SmartContract;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Neo.Plugins.Cache
{
    public class CacheContractParams
    {
        public UInt160 Hash;
        public ulong Time;
        public UInt256 Txid;
    }

    public class CacheContract : IDBCache
    {
        private ConcurrentDictionary<UInt160, CacheContractParams> D_Contract;
        private ConcurrentDictionary<UInt160, ContractModel> D_ContractModel;

        public CacheContract()
        {
            D_ContractModel = new ConcurrentDictionary<UInt160, ContractModel>();
            D_Contract = new ConcurrentDictionary<UInt160, CacheContractParams>();
        }

        public void Clear()
        {
            D_ContractModel = new ConcurrentDictionary<UInt160, ContractModel>();
            D_Contract = new ConcurrentDictionary<UInt160, CacheContractParams>();
        }

        public void AddNeedUpdate(UInt160 contractHash, ulong createTime, UInt256 txid)
        {
            D_Contract[contractHash] = new() { Hash = contractHash, Time = createTime, Txid = txid };
        }

        public List<CacheContractParams> GetNeedUpdate()
        {
            return D_Contract.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheContractParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                AddOrUpdate(list[i].Hash, snapshot, list[i].Time, list[i].Txid);
            });
        }

        public ContractModel Get(UInt160 contractHash)
        {
            if (D_ContractModel.ContainsKey(contractHash))
            {
                return D_ContractModel[contractHash];
            }
            else
            {
                return ContractModel.Get(contractHash);
            }
        }

        public void AddOrUpdate(UInt160 contractHash, DataCache snapshot, ulong createTime, UInt256 txid)
        {
            ContractModel contractModel = Get(contractHash);
            if (contractModel is null)
            {
                StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.ContractManagement.Id, 8).Add(contractHash);
                ContractState contract = snapshot.TryGet(key)?.GetInteroperable<ContractState>();
                contractModel = new(contractHash, contract.Manifest.Name, contract.Id, contract.UpdateCounter, contract.Nef.ToJson(), contract.Manifest.ToJson(), createTime, txid);
            }
            else
            {
                if (contractModel.CreateTime == createTime)
                    return;
                StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.ContractManagement.Id, 8).Add(contractHash);
                ContractState contract = snapshot.TryGet(key)?.GetInteroperable<ContractState>();
                if (contractModel.UpdateCounter == contract.UpdateCounter)
                    return;
                contractModel = new(contractHash, contract.Manifest.Name, contract.Id, contract.UpdateCounter, contract.Nef.ToJson(), contract.Manifest.ToJson(), createTime, txid);
            }
            D_ContractModel[contractHash] = contractModel;
        }

        public void Save(Transaction tran)
        {
            if (D_ContractModel.Values.Count > 0)
                tran.SaveAsync(D_ContractModel.Values).Wait();
        }
    }
}

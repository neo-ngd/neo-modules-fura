using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Neo.Cryptography;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract;
using Neo.SmartContract.Native;

namespace Neo.Plugins
{
    public class NotificationMgr
    {
        Dictionary<UInt256, Func<NotificationModel, NeoSystem, Block, DataCache, bool>> dic_filter = new Dictionary<UInt256, Func<NotificationModel, NeoSystem, Block, DataCache, bool>>();

        private static NotificationMgr ins;

        private static Dictionary<UInt160, EnumAssetType> dic_AssetType = new Dictionary<UInt160, EnumAssetType>();

        private static object lockObj = new object();

        public static NotificationMgr Ins
        {
            get
            {
                if (ins is null)
                {
                    lock (lockObj)
                    {
                        if (ins is null)
                            ins = new NotificationMgr();
                    }
                }
                return ins;
            }

        }

        public NotificationMgr()
        {
            Register("Transfer", ExecuteTransferNotification);
            Register("Deploy", ExecuteDeployNotification);
            Register("Update", ExecuteUpdateNotification);
        }

        public void Register(string eventName, Func<NotificationModel, NeoSystem, Block, DataCache, bool> entity)
        {
            dic_filter.Add(GetKey(eventName), entity);
        }

        public UInt256 GetKey(string eventName)
        {
            return new UInt256((UTF8Encoding.UTF8.GetBytes(eventName)).Sha256());
        }

        public void Filter(List<NotificationModel> notificationModels, NeoSystem system, Block block, DataCache snapshot)
        {
            Parallel.For(0, notificationModels.Count, (i) =>
            {
                var notificationModel = notificationModels[i];
                if (notificationModel.Vmstate != "HALT")
                {
                    return;
                }
                var key = GetKey(notificationModel.EventName);
                if (dic_filter.ContainsKey(key))
                {
                    dic_filter[key](notificationModel, system, block, snapshot);
                }
            });
        }

        private bool ExecuteTransferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if (notificationModel.State.Values.Count() == 3)
            {
                var t = ExecuteNep17TransferNotification(notificationModel, system, block, snapshot);
                //如果是neo的转账，需要整一个vote的记录
                if (notificationModel.ContractHash == NeoToken.NEO.Hash)
                {
                    UpdateVoteModelByTransfer(notificationModel.Txid, t.Item1, system, block, snapshot);
                    UpdateVoteModelByTransfer(notificationModel.Txid, t.Item2, system, block, snapshot);
                }
                //gas的一些特殊处理，例如如果是gas的销毁转账，记录一个销毁记录
                if (notificationModel.ContractHash == GasToken.GAS.Hash)
                {
                    ExecuteGasSepical(notificationModel, system, block, snapshot);
                }
            }
            else if (notificationModel.State.Values.Count() == 4)
            {
                ExecuteNep11TransferNotification(notificationModel, system, block, snapshot);
            }
            return true;
        }

        private void UpdateVoteModelByTransfer(UInt256 txid, UInt160 voter, NeoSystem system, Block block, DataCache snapshot)
        {
            if (voter == UInt160.Zero || voter is null) { return; }
            var voteModel = DBCache.Ins.cacheVote.Get(voter);
            if (voteModel is null)
                return;
            //增加需要更新的voter的信息
            DBCache.Ins.cacheVote.AddNeedUpdate(voter, null, txid, block.Index, voteModel.Candidate, voteModel.CandidatePubKey);
            DBCache.Ins.cacheCandidate.AddNeedUpdate(voteModel.Candidate, voteModel.CandidatePubKey, EnumCandidateState.Unknow);
        }

        private void ExecuteGasSepical(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if(notificationModel.State.Values[1].Value is null)//gas 销毁
            {
                BigInteger value = 0;
                BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
                DBCache.Ins.cacheGasMintBurn.Add(block.Index,value, 0);
            }
            if (notificationModel.State.Values[0].Value is null)//gas 增发
            {
                BigInteger value = 0;
                BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
                DBCache.Ins.cacheGasMintBurn.Add(block.Index,0, value);
            }
        }

        private (UInt160,UInt160) ExecuteNep17TransferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            EnumAssetType assetType = GetAssetType(snapshot, notificationModel.ContractHash);
            if (assetType != EnumAssetType.NEP17)
                return (UInt160.Zero, UInt160.Zero);

            UInt256 txid = notificationModel.Txid;
            UInt256 blockHash = notificationModel.BlockHash;
            ulong timestamp = notificationModel.Timestamp;
            UInt160 from = null;
            UInt160 to = null;
            BigInteger value = 0;
            bool succ = true;
            if (notificationModel.State.Values[0].Value is not null)
            {
                succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out from);
            }
            if (notificationModel.State.Values[1].Value is not null)
            {
                succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out to);
            }
            succ = succ && BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
            if (!succ)
            {
                return (UInt160.Zero, UInt160.Zero);
            }
            BigInteger[] balances = Neo.Plugins.VM.Helper.GetNep17BalanceOf(system, snapshot, notificationModel.ContractHash, from, to);
            DBCache.Ins.cacheTransferNotification.Add(txid, blockHash, timestamp, notificationModel.ContractHash, from, to, value, balances[0], balances[1]);
            DBCache.Ins.cacheAddressAsset.AddNeedUpdate(from, notificationModel.ContractHash, "");
            DBCache.Ins.cacheAddressAsset.AddNeedUpdate(to, notificationModel.ContractHash, "");
            DBCache.Ins.cacheAddress.AddNeedUpdate(from, block.Timestamp);
            DBCache.Ins.cacheAddress.AddNeedUpdate(to, block.Timestamp);

            if (from == UInt160.Zero || from is null || to == UInt160.Zero || to is null) //如果from为0x0，意味着发行代币，如果to为0x0，意味着销毁代币，这个时候需要更新资产的总量
            {
                DBCache.Ins.cacheAsset.AddNeedUpdate(notificationModel.ContractHash, block.Timestamp, EnumAssetType.NEP17);
            }
            return (from, to);
        }

        private void ExecuteNep11TransferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            EnumAssetType assetType = GetAssetType(snapshot, notificationModel.ContractHash);
            if (assetType != EnumAssetType.NEP11)
                return ;
            UInt256 txid = notificationModel.Txid;
            UInt256 blockHash = notificationModel.BlockHash;
            ulong timestamp = notificationModel.Timestamp;

            UInt160 from = null;
            UInt160 to = null;
            BigInteger value = 0;
            bool succ = true;
            if (notificationModel.State.Values[0].Value is not null)
            {
                succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out from);
            }
            if (notificationModel.State.Values[1].Value is not null)
            {
                succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out to);
            }
            succ = succ && BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
            string tokenId = "";
            if (notificationModel.State.Values[3].Type == "Integer")  //需要转换一下
            {
                tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[3].Value).ToByteArray());
            }
            else
            {
                tokenId = notificationModel.State.Values[3].Value;
            }

            if (!succ)
            {
                return;
            }

            var balance_from = Neo.Plugins.VM.Helper.GetNep11BalanceOf(system, snapshot, notificationModel.ContractHash, tokenId, from);
            var balance_to = Neo.Plugins.VM.Helper.GetNep11BalanceOf(system, snapshot, notificationModel.ContractHash, tokenId, to);

            DBCache.Ins.cacheNep11TransferNotification.Add(txid, blockHash, timestamp, notificationModel.ContractHash, tokenId, from, to, value, balance_from, balance_to);

            DBCache.Ins.cacheAddressAsset.AddNeedUpdate(from, notificationModel.ContractHash, tokenId);
            DBCache.Ins.cacheAddressAsset.AddNeedUpdate(to, notificationModel.ContractHash, tokenId);

            DBCache.Ins.cacheAddress.Add(block.Timestamp, from, to);
            DBCache.Ins.cacheNep11Properties.AddNeedUpdate(notificationModel.ContractHash, tokenId);
            if (from == UInt160.Zero || from is null || to == UInt160.Zero || to is null) //如果from为0x0，意味着发行代币，如果to为0x0，意味着销毁代币，这个时候需要更新资产的总量
            {
                DBCache.Ins.cacheAsset.AddNeedUpdate(notificationModel.ContractHash, block.Timestamp, EnumAssetType.NEP11);
            }
        }

        private bool ExecuteDeployNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if(notificationModel.ContractHash == NativeContract.ContractManagement.Hash)
            {
                UInt160 contractHash = null;
                bool succ = UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out contractHash);
                if (!succ) return false;
                DBCache.Ins.cacheContract.AddNeedUpdate(contractHash, block.Timestamp, notificationModel.Txid);
                //如果合约还是asset，也一并更新了
                EnumAssetType assetType = GetAssetType(snapshot, contractHash);
                if (assetType is EnumAssetType.NEP11 || assetType is EnumAssetType.NEP17)
                {
                    DBCache.Ins.cacheAsset.AddNeedUpdate(contractHash, block.Timestamp, assetType);
                }
            }
            return true;
        }

        private bool ExecuteUpdateNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if (notificationModel.ContractHash == NativeContract.ContractManagement.Hash)
            {
                UInt160 contractHash = null;
                bool succ = UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out contractHash);
                if (!succ) return false;
                DBCache.Ins.cacheContract.AddNeedUpdate(contractHash, block.Timestamp, notificationModel.Txid);
                //如果合约还是asset，也一并更新了
                EnumAssetType assetType = GetAssetType(snapshot, contractHash);
                if (assetType is EnumAssetType.NEP11 || assetType is EnumAssetType.NEP17)
                {
                    DBCache.Ins.cacheAsset.AddNeedUpdate(contractHash, block.Timestamp, assetType);
                }
            }
            return true;
        }

        private EnumAssetType GetAssetType(DataCache snapshot, UInt160 hash)
        {
            if (dic_AssetType.ContainsKey(hash))
                return dic_AssetType[hash];
            StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.ContractManagement.Id, 8).Add(hash);
            ContractState contract = snapshot.TryGet(key)?.GetInteroperable<ContractState>();
            EnumAssetType assetType;
            if (contract is null)
            {
                assetType = EnumAssetType.Unknown;
            }
            else if (contract.Manifest.SupportedStandards.Contains("NEP-17"))
            {
                assetType = EnumAssetType.NEP17;
            }
            else if (contract.Manifest.SupportedStandards.Contains("NEP-11"))
            {
                assetType = EnumAssetType.NEP11;
            }
            else
            {
                assetType = EnumAssetType.Unknown;
            }
            lock (dic_AssetType)
            {
                if (!dic_AssetType.ContainsKey(hash))
                    dic_AssetType.Add(hash, assetType);
            }
            return assetType;
        }
    }
}

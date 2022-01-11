using System;
using System.Linq;
using System.Numerics;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
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
            if (notificationModel.State.Values[1].Value is null)//gas 销毁
            {
                BigInteger value = 0;
                BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
                DBCache.Ins.cacheGasMintBurn.Add(block.Index, value, 0);
            }
            if (notificationModel.State.Values[0].Value is null)//gas 增发
            {
                BigInteger value = 0;
                BigInteger.TryParse(notificationModel.State.Values[2].Value, out value);
                DBCache.Ins.cacheGasMintBurn.Add(block.Index, 0, value);
            }
        }

        private (UInt160, UInt160) ExecuteNep17TransferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
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

            if (from == UInt160.Zero || from is null) //如果from为0x0，意味着发行代币，这个时候需要更新资产的总量
            {
                DBCache.Ins.cacheAsset.AddNeedUpdate(notificationModel.ContractHash, block.Timestamp, EnumAssetType.NEP17);
            }
            return (from, to);
        }

        private void ExecuteNep11TransferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            EnumAssetType assetType = GetAssetType(snapshot, notificationModel.ContractHash);
            if (assetType != EnumAssetType.NEP11)
                return;
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

            DBCache.Ins.cacheMarket.AddNeedUpdate(notificationModel.Index, true, notificationModel.ContractHash, from, tokenId, block.Timestamp);
            DBCache.Ins.cacheMarket.AddNeedUpdate(notificationModel.Index, true, notificationModel.ContractHash, to, tokenId, block.Timestamp);

            DBCache.Ins.cacheAddress.Add(block.Timestamp, from, to);
            DBCache.Ins.cacheNep11Properties.AddNeedUpdate(notificationModel.ContractHash, tokenId);
            if (from == UInt160.Zero || from is null) //如果from为0x0，意味着发行代币，这个时候需要更新资产的总量
            {
                DBCache.Ins.cacheAsset.AddNeedUpdate(notificationModel.ContractHash, block.Timestamp, EnumAssetType.NEP11);
            }
        }
    }
}


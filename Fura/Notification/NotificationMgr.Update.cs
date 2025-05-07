using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using Neo.Extensions;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
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
    }
}


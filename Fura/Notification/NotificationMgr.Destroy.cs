using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        private bool ExecuteDestroyNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if (notificationModel.ContractHash == NativeContract.ContractManagement.Hash)
            {
                UInt160 contractHash = null;
                bool succ = UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out contractHash);
                if (!succ) return false;
                DBCache.Ins.cacheContract.AddNeedUpdate(contractHash, block.Timestamp, notificationModel.Txid, true);
            }
            return true;
        }
    }
}


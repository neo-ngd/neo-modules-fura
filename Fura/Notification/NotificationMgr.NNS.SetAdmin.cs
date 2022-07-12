using System;
using System.Linq;
using System.Numerics;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;

namespace Neo.Plugins.Notification
{
	public partial class NotificationMgr
	{
        private bool ExecuteSetAdminNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            if (UInt160.Parse(Settings.Default.NNS) == notificationModel.ContractHash)
            {
                bool succ = true;
                string tokenId = "";
                if (notificationModel.State.Values[0].Type == "Integer")  //需要转换一下
                {
                    tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[0].Value).ToByteArray());
                }
                else
                {
                    tokenId = notificationModel.State.Values[0].Value;
                }

                if (!succ)
                {
                    return succ;
                }
                DBCache.Ins.cacheNep11Properties.AddNeedUpdate(notificationModel.ContractHash, tokenId);
            }
            return true;
        }
    }
}


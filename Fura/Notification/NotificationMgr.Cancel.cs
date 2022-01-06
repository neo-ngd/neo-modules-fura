using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using System.Numerics;
using Neo.IO.Json;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        private bool ExecuteCancelNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel = DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (Settings.Default.MarketContractIds.Contains(contractModel._ID))
            {
                BigInteger nonce = 0;
                UInt160 user = null;
                UInt160 asset = null;
                string tokenId = "";
                bool succ = true;
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[0].Value, out nonce);
                //user
                if (notificationModel.State.Values[1].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out user);
                }
                //asset
                if (notificationModel.State.Values[2].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[2].Value).Reverse().ToArray().ToHexString(), out asset);
                }
                //tokenid
                if (notificationModel.State.Values[3].Type == "Integer")  //需要转换一下
                {
                    tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[3].Value).ToByteArray());
                }
                else
                {
                    tokenId = notificationModel.State.Values[3].Value;
                }
                //暴露出通知的时候，nft的所有者已经变成了原先的用户了。
                MarketModel marketModel = DBCache.Ins.cacheMarket.Get(notificationModel.ContractHash, asset, tokenId);
                DBCache.Ins.cacheMarket.AddNeedUpdate(false, asset, notificationModel.ContractHash, tokenId, null, 0, null, null, 0, 0, null, 0, block.Timestamp);

                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, notificationModel.ContractHash, nonce, user, user, tokenId, "Cancel", "{}", notificationModel.Timestamp);
            }
            return true;
        }
    }
}


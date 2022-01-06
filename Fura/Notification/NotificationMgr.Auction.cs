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
        private bool ExecuteAuctionNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel= DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (Settings.Default.MarketContractIds.Contains(contractModel._ID))
            {
                BigInteger nonce = 0;
                UInt160 user = null;
                UInt160 asset = null;
                string tokenId = "";
                BigInteger auctionType = 0;
                UInt160 auctionAsset = null;
                BigInteger auctionAmount = 0;
                BigInteger deadline = 0;
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
                //type
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[4].Value, out auctionType);
                //auctionAsset
                if (notificationModel.State.Values[5].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[5].Value).Reverse().ToArray().ToHexString(), out auctionAsset);
                }
                //auctionAmount
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[6].Value, out auctionAmount);
                //deadline
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[7].Value, out deadline);

                //暴露出通知的时候，nft的所有者已经变成了market了。
                DBCache.Ins.cacheMarket.AddNeedUpdate(false, asset, notificationModel.ContractHash, tokenId, notificationModel.ContractHash, auctionType, user, auctionAsset, auctionAmount, deadline, null, 0, block.Timestamp);
                JObject json = new JObject();
                json["auctionType"] = auctionType.ToString();
                json["auctionAsset"] = auctionAsset?.ToString();
                json["auctionAmount"] = auctionAmount.ToString();
                json["deadline"] = deadline.ToString();
                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, notificationModel.ContractHash, nonce, user, asset, tokenId, "Auction", json.ToString(), notificationModel.Timestamp);
            }
            return true;
        }
    }
}


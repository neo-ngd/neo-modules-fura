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
        private bool ExecuteClaimNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel = DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (contractModel._ID == Settings.Default.MarketContractId && notificationModel.State.Values.Count() == 6)
            {
                UInt160 user = null;
                UInt160 asset = null;
                string tokenId = "";
                BigInteger auctionType = 0;
                UInt160 auctionAsset = null;
                BigInteger bidAmount = 0;
                bool succ = true;
                //user
                if (notificationModel.State.Values[0].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out user);
                }
                //asset
                if (notificationModel.State.Values[1].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out asset);
                }
                //tokenid
                if (notificationModel.State.Values[2].Type == "Integer")  //需要转换一下
                {
                    tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[2].Value).ToByteArray());
                }
                else
                {
                    tokenId = notificationModel.State.Values[2].Value;
                }
                //type
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[3].Value, out auctionType);
                //auctionAsset
                if (notificationModel.State.Values[4].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[4].Value).Reverse().ToArray().ToHexString(), out auctionAsset);
                }
                //auctionAmount
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[5].Value, out bidAmount);

                MarketModel marketModel = DBCache.Ins.cacheMarket.Get(notificationModel.ContractHash, asset, tokenId);
                DBCache.Ins.cacheMarket.AddNeedUpdate(false, asset, notificationModel.ContractHash, tokenId, null, 0, null, null, 0, 0, null, 0);

                JObject json = new JObject();
                json["user"] = user?.ToString();
                json["asset"] = asset?.ToString();
                json["tokenId"] = tokenId;
                json["auctionType"] = auctionType.ToString();
                json["auctionAsset"] = auctionAsset?.ToString();
                json["bidAmount"] = bidAmount.ToString();
                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, user, "Claim", json.ToString(), notificationModel.Timestamp);
            }
            return true;
        }
    }
}
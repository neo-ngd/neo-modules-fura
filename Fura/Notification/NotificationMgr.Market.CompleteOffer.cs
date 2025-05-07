using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using System.Numerics;
using Neo.Json;
using Neo.Extensions;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        private bool ExecuteCompleteOfferNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel = DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (Settings.Default.MarketContractIds.Contains(contractModel.ContractId))
            {
                //(nonce, 用户 ,求购使用的nep17资产，nep17数额，求购的nft的hash，求购的nfttokenid，求购截止日期)
                BigInteger nonce = 0;
                UInt160 offerer = null;
                UInt160 user = null;
                UInt160 offerAsset = null;
                BigInteger offerAmount = 0;
                UInt160 asset = null;
                string tokenId = "";
                BigInteger endTimestamp = 0;
                bool succ = true;
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[0].Value, out nonce);
                //user
                if (notificationModel.State.Values[1].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out user);
                }
                //offerer
                if (notificationModel.State.Values[2].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[2].Value).Reverse().ToArray().ToHexString(), out offerer);
                }
                //offerAsset
                if (notificationModel.State.Values[3].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[3].Value).Reverse().ToArray().ToHexString(), out offerAsset);
                }
                //offerAmount
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[4].Value, out offerAmount);
                //asset
                if (notificationModel.State.Values[5].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[5].Value).Reverse().ToArray().ToHexString(), out asset);
                }
                //tokenid
                if (notificationModel.State.Values[6].Type == "Integer")  //需要转换一下
                {
                    tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[6].Value).ToByteArray());
                }
                else
                {
                    tokenId = notificationModel.State.Values[6].Value;
                }
                //endtimestamp
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[7].Value, out endTimestamp);


                JObject json = new JObject();
                json["offerer"] = offerer?.ToString();
                json["offerAsset"] = offerAsset?.ToString();
                json["offerAmount"] = offerAmount.ToString();
                json["deadline"] = endTimestamp.ToString();
                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, notificationModel.ContractHash, nonce, user, asset, tokenId, "CompleteOffer", json.ToString(), notificationModel.Timestamp);
            }
            return true;
        }
    }
}


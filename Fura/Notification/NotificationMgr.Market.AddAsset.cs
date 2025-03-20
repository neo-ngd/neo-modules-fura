using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using System.Numerics;
using Neo.Json;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        private bool ExecuteAddAssetNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel = DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (Settings.Default.MarketContractIds.Contains(contractModel.ContractId))
            {
                UInt160 asset = null;
                BigInteger _feeRate = 0;
                BigInteger _rewardRate = 0;
                UInt160 _rewardReceiveAddress = null;
                bool succ = true;
                //asset
                if (notificationModel.State.Values[0].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[0].Value).Reverse().ToArray().ToHexString(), out asset);
                }
                //_feeRate
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[1].Value, out _feeRate);
                //_rewardRate
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[2].Value, out _rewardRate);
                //_rewardReceiveAddress
                if (notificationModel.State.Values[3].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[3].Value).Reverse().ToArray().ToHexString(), out _rewardReceiveAddress);
                }

                JObject json = new JObject();
                json["feeRate"] = _feeRate.ToString();
                json["rewardRate"] = _rewardRate.ToString();
                json["rewardReceiveAddress"] = _rewardReceiveAddress.ToString();
                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, notificationModel.ContractHash, 0, null, asset, "", "AddAsset", json.ToString(), notificationModel.Timestamp);
            }
            return true;
        }
    }
}


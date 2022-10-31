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

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        Dictionary<UInt256, Func<NotificationModel, NeoSystem, Block, DataCache, bool>> dic_filter = new Dictionary<UInt256, Func<NotificationModel, NeoSystem, Block, DataCache, bool>>();

        private static NotificationMgr ins;

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
            Register("Destroy", ExecuteDestroyNotification);

            Register("Auction", ExecuteAuctionNotification);
            Register("Bid", ExecuteBidNotification);
            Register("Cancel", ExecuteCancelNotification);
            Register("Claim", ExecuteClaimNotification);
            Register("AddAsset", ExecuteAddAssetNotification);
            Register("RemoveAsset", ExecuteRemoveAssetNotification);
            Register("Offer", ExecuteOfferNotification);
            Register("CancelOffer", ExecuteCancelOfferNotification);
            Register("CompleteOffer", ExecuteCompleteOfferNotification);
            Register("OfferCollection", ExecuteOfferCollectionNotification);
            Register("CancelOfferCollection", ExecuteCancelOfferCollectionNotification);
            Register("CompleteOfferCollection", ExecuteCompleteOfferCollectionNotification);



            Register("SetAdmin", ExecuteSetAdminNotification);
            Register("Renew", ExecuteRenewNotification);
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

        private EnumAssetType GetAssetType(DataCache snapshot, UInt160 hash)
        {
            StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.ContractManagement.Id, 8).Add(hash);
            ContractState contract = snapshot.TryGet(key)?.GetInteroperable<ContractState>();
            EnumAssetType assetType;
            if (contract is null)
            {
                assetType = EnumAssetType.Unknown;
            }
            else if (contract.Manifest.SupportedStandards.Contains("NEP-17") || Settings.Default.Nep17ContractIds.Contains(contract.Id))
            {
                assetType = EnumAssetType.NEP17;
            }
            else if (contract.Manifest.SupportedStandards.Contains("NEP-11") || Settings.Default.Nep11ContractIds.Contains(contract.Id))
            {
                assetType = EnumAssetType.NEP11;
            }
            else
            {
                assetType = EnumAssetType.Unknown;
            }
            return assetType;
        }
    }
}

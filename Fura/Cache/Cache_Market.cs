using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace Neo.Plugins.Cache
{
    public class CacheMarketParams
    {
        public bool SimpleUpdate;
        public UInt160 Owner;
        public UInt160 Asset;
        public string TokenId;
        public BigInteger Amount;
        public UInt160 Market;
        public uint AuctionType;
        public UInt160 Auctor;
        public UInt160 AuctionAsset;
        public BigInteger AuctionAmount;
        public ulong Deadline;
        public UInt160 Bidder;
        public BigInteger BidAmount;
    }

    public class CacheMarket : IDBCache
    {
        private ConcurrentDictionary<(UInt160, UInt160, string), CacheMarketParams> D_Market;

        private ConcurrentDictionary<(UInt160, UInt160, string), MarketModel> D_MarketModel;

        public void AddNeedUpdate(bool SimpleUpdate, UInt160 asset, UInt160 owner, string tokenid)
        {
            D_Market[(asset, owner, tokenid)] = new()
            {
                Asset = asset,
                TokenId = tokenid,
                Owner = owner
            };
        }

        public void AddNeedUpdate(bool simpleUpdate, UInt160 asset, UInt160 owner, string tokenid, UInt160 market, BigInteger auctionType, UInt160 auctor, UInt160 auctionAsset, BigInteger auctionAmount, BigInteger deadline, UInt160 bidder, BigInteger bidAmount)
        {
            D_Market[(asset, owner, tokenid)] = new()
            {
                SimpleUpdate = simpleUpdate,
                Asset = asset,
                TokenId = tokenid,
                Owner = owner,
                Market = market,
                AuctionType = (uint)auctionType,
                Auctor = auctor,
                AuctionAsset = auctionAsset,
                AuctionAmount = auctionAmount,
                Deadline = (ulong)deadline,
                Bidder = bidder,
                BidAmount = bidAmount
            };
        }

        public List<CacheMarketParams> GetNeedUpdate()
        {
            return D_Market.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheMarketParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                list[i].Amount = Neo.Plugins.VM.Helper.GetNep11BalanceOf(system, snapshot, list[i].Asset, list[i].TokenId, list[i].Owner);
                AddOrUpdate(list[i]);
            });
        }

        public MarketModel Get(UInt160 owner, UInt160 asset, string tokenid)
        {
            if (D_MarketModel.ContainsKey((owner, asset, tokenid)))
            {
                return D_MarketModel[(owner, asset, tokenid)];
            }
            else
            {
                return MarketModel.Get(owner, asset, tokenid);
            }
        }

        public void AddOrUpdate(CacheMarketParams cacheMarketParams)
        {
            if (cacheMarketParams.Owner == null || cacheMarketParams.Asset == null)
                return;
            MarketModel marketModel = Get(cacheMarketParams.Owner, cacheMarketParams.Asset, cacheMarketParams.TokenId);
            if (marketModel is null)
            {
                marketModel = new MarketModel()
                {
                    Asset = cacheMarketParams.Asset,
                    TokenId = cacheMarketParams.TokenId,
                    Owner = cacheMarketParams.Owner,
                    Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString()),
                    Market = cacheMarketParams.Market,
                    AuctionType = cacheMarketParams.AuctionType,
                    AuctionAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.AuctionAmount.ToString()),
                    AuctionAsset = cacheMarketParams.AuctionAsset,
                    Auctor = cacheMarketParams.Auctor,
                    BidAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.BidAmount.ToString()),
                    Bidder = cacheMarketParams.Bidder,
                    Deadline = cacheMarketParams.Deadline,
                };
            }
            else if(cacheMarketParams.SimpleUpdate)
            {
                marketModel.Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString());
            }
            else
            {
                marketModel.Asset = cacheMarketParams.Asset;
                marketModel.TokenId = cacheMarketParams.TokenId;
                marketModel.Owner = cacheMarketParams.Owner;
                marketModel.Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString());
                marketModel.Market = cacheMarketParams.Market;
                marketModel.AuctionType = cacheMarketParams.AuctionType;
                marketModel.AuctionAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.AuctionAmount.ToString());
                marketModel.AuctionAsset = cacheMarketParams.AuctionAsset;
                marketModel.Auctor = cacheMarketParams.Auctor;
                marketModel.BidAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.BidAmount.ToString());
                marketModel.Bidder = cacheMarketParams.Bidder;
                marketModel.Deadline = cacheMarketParams.Deadline;
            }
            D_MarketModel[((marketModel.Owner, marketModel.Asset, marketModel.TokenId))] = marketModel;
        }

        public void Clear()
        {
            D_Market = new ConcurrentDictionary<(UInt160, UInt160, string), CacheMarketParams>();
            D_MarketModel = new ConcurrentDictionary<(UInt160, UInt160, string), MarketModel>();
        }

        public void Save(Transaction tran)
        {
            if (D_MarketModel.Values.Count > 0)
                tran.SaveAsync(D_MarketModel.Values).Wait();
        }
    }
}


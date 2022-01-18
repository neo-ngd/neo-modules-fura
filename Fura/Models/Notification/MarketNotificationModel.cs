using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("MarketNotification")]
    public class MarketNotificationModel : Entity
    {
        [BsonElement("nonce")]
        public ulong Nonce { get; set; }

        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [UInt160AsString]
        [BsonElement("user")]
        public UInt160 User { get; set; }

        [UInt160AsString]
        [BsonElement("market")]
        public UInt160 Market { get; set; }

        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenId { get; set; }

        [BsonElement("eventname")]
        public string EventName { get; set; }

        [BsonElement("extendData")]
        public string ExtendData { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        public MarketNotificationModel(UInt256 txid, UInt256 blockhash, BigInteger nonce, UInt160 user, UInt160 market, UInt160 asset, string tokenId, string eventName, string extendData, ulong timestamp)
        {
            Nonce = (ulong)nonce;
            Txid = txid;
            BlockHash = blockhash;
            User = user;
            Market = market;
            Asset = asset;
            TokenId = tokenId;
            EventName = eventName;
            ExtendData = extendData;
            Timestamp = timestamp;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<MarketNotificationModel>(new CreateCollectionOptions<MarketNotificationModel>());

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Nonce, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_nonce_asset_tokenid_eventname_timestamp_market_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Nonce, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_nonce_asset_eventname_timestamp_market_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Nonce, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Key(a => a.User, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_nonce_asset_eventname_timestamp_user_market_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Nonce, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.User, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_nonce_asset_user_eventname_market_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Nonce, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.User, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_nonce_asset_user_eventname_tokenid_market_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Option(o => { o.Name = "_eventname_market_asset_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.EventName, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_tokenid_market_eventname_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Option(o => { o.Name = "_market_timestamp_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.User, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Option(o => { o.Name = "_user_market_timestamp_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Option(o => { o.Name = "_eventname_asset_tokenid_timestamp_"; }).CreateAsync();

            await DB.Index<MarketNotificationModel>()
                .Key(a => a.EventName, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Timestamp, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_eventname_asset_tokenid_market_timestamp_"; }).CreateAsync();
        }
    }
}

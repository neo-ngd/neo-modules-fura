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
            await DB.Index<MarketNotificationModel>().Key(a => a.User, KeyType.Ascending).Option(o => { o.Name = "_user_"; }).CreateAsync();
            await DB.Index<MarketNotificationModel>().Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_timestamp_"; }).CreateAsync();
            await DB.Index<MarketNotificationModel>().Key(a => a.EventName, KeyType.Ascending).Option(o => { o.Name = "_eventname_"; }).CreateAsync();
            await DB.Index<MarketNotificationModel>().Key(a => a.User, KeyType.Ascending).Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_user_timestamp_"; }).CreateAsync();
            await DB.Index<MarketNotificationModel>().Key(a => a.User, KeyType.Ascending).Key(a => a.EventName, KeyType.Ascending).Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_user_eventname_timestamp_"; }).CreateAsync();
        }
    }
}

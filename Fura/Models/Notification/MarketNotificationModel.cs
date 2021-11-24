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
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [UInt160AsString]
        [BsonElement("user")]
        public UInt160 User { get; set; }

        [BsonElement("eventname")]
        public string EventName { get; set; }

        [BsonElement("params")]
        public string Params { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        public MarketNotificationModel(UInt256 txid, UInt256 blockhash, UInt160 user, string eventName, string @params, ulong timestamp)
        {
            Txid = txid;
            BlockHash = blockhash;
            User = user;
            EventName = eventName;
            Params = @params;
            timestamp = Timestamp;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<MarketNotificationModel>(new CreateCollectionOptions<MarketNotificationModel>());
            await DB.Index<MarketNotificationModel>().Key(a => a.User, KeyType.Ascending).Option(o => { o.Name = "_user_"; }).CreateAsync();
            await DB.Index<MarketNotificationModel>().Key(a => a.User, KeyType.Ascending).Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_user_timestamp_"; }).CreateAsync();
        }
    }
}

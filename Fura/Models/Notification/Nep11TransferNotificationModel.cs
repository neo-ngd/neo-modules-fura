using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Nep11TransferNotification")]
    public class Nep11TransferNotificationModel : Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [UInt160AsString]
        [BsonElement("contract")]
        public UInt160 AssetHash { get; set; }

        [BsonElement("tokenId")]
        public string TokenId { get; set; }

        [UInt160AsString]
        [BsonElement("from")]
        public UInt160 From { get; set; }

        [UInt160AsString]
        [BsonElement("to")]
        public UInt160 To { get; set; }

        [BsonElement("value")]
        public BsonDecimal128 Value { get; set; }

        [BsonElement("frombalance")]
        public BsonDecimal128 FromBalanceOf { get; set; }

        [BsonElement("tobalance")]
        public BsonDecimal128 ToBalanceOf { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        public Nep11TransferNotificationModel() { }

        public Nep11TransferNotificationModel(UInt256 txid, UInt256 blockHash, ulong timestamp, UInt160 assetHash, string tokenId, UInt160 from, UInt160 to, BigInteger value, BigInteger fromBalanceOf, BigInteger toBalanceOf)
        {
            Txid = txid;
            BlockHash = blockHash;
            AssetHash = assetHash;
            TokenId = tokenId;
            From = from;
            To = to;
            Value = BsonDecimal128.Create(value.ToString().WipeNumStrToFitDecimal128());
            FromBalanceOf = BsonDecimal128.Create(fromBalanceOf.ToString().WipeNumStrToFitDecimal128());
            ToBalanceOf = BsonDecimal128.Create(toBalanceOf.ToString().WipeNumStrToFitDecimal128());
            Timestamp = timestamp;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<Nep11TransferNotificationModel>(new CreateCollectionOptions<Nep11TransferNotificationModel>());
            await DB.Index<Nep11TransferNotificationModel>().Key(a => a.To, KeyType.Ascending).Option(o => { o.Name = "_to_"; }).CreateAsync();
            await DB.Index<Nep11TransferNotificationModel>().Key(a => a.AssetHash, KeyType.Ascending).Option(o => { o.Name = "_contract_"; }).CreateAsync();
            await DB.Index<Nep11TransferNotificationModel>().Key(a => a.AssetHash, KeyType.Ascending).Key(a => a.To, KeyType.Ascending).Option(o => { o.Name = "_contract_to_"; }).CreateAsync();
            await DB.Index<Nep11TransferNotificationModel>().Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_timestamp_"; }).CreateAsync();
            await DB.Index<Nep11TransferNotificationModel>().Key(a => a.Txid, KeyType.Ascending).Key(a => a.Timestamp, KeyType.Ascending).Option(o => { o.Name = "_timestamp_txid_"; }).CreateAsync();
        }
    }
}

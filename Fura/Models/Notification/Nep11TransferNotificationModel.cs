using System.Numerics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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

        public Nep11TransferNotificationModel(UInt256 txid, UInt256 blockHash, ulong timestamp, UInt160 assetHash, string tokenId, UInt160 from, UInt160 to, BigInteger value, BigInteger fromBalanceOf, BigInteger toBalanceOf)
        {
            Txid = txid;
            BlockHash = blockHash;
            AssetHash = assetHash;
            TokenId = tokenId;
            From = from;
            To = to;
            Value = BsonDecimal128.Create(value.ToString());
            FromBalanceOf = BsonDecimal128.Create(fromBalanceOf.ToString());
            ToBalanceOf = BsonDecimal128.Create(toBalanceOf.ToString());
            Timestamp = timestamp;
        }
    }
}

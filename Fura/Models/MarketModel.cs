using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using System.Numerics;
using System.Threading.Tasks;

namespace Neo.Plugins.Models
{
    [Collection("Market")]
    public class MarketModel : Entity
    {
        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenId { get; set; }

        [BsonElement("amount")]
        public BsonDecimal128 Amount { get; set; }

        [UInt160AsString]
        [BsonElement("owner")]
        public UInt160 Owner { get; set; }

        [UInt160AsString]
        [BsonElement("market")]
        public UInt160 Market { get; set; }

        [BsonElement("auctionType")]
        public uint AuctionType { get; set; }

        [UInt160AsString]
        [BsonElement("auctor")]
        public UInt160 Auctor { get; set; }

        [UInt160AsString]
        [BsonElement("auctionAsset")]
        public UInt160 AuctionAsset { get; set; }

        [BsonElement("auctionAmount")]
        public BsonDecimal128 AuctionAmount { get; set; }

        [BsonElement("deadline")]
        public ulong Deadline { get; set; }

        [UInt160AsString]
        [BsonElement("bidder")]
        public UInt160 Bidder { get; set; }

        [BsonElement("bidAmount")]
        public BsonDecimal128 BidAmount { get; set; }

        public static MarketModel Get(UInt160 owner, UInt160 asset, string tokenid)
        {
            MarketModel marketModel = DB.Find<MarketModel>().Match(a => a.Owner == owner && a.Asset == asset && a.TokenId == tokenid).ExecuteFirstAsync().Result;
            return marketModel;
        }

        public static void Delete(UInt160 owner, UInt160 asset, string tokenid)
        {
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<MarketModel>(new CreateCollectionOptions<MarketModel>());
            await DB.Index<MarketModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.Owner, KeyType.Ascending).Key(a => a.TokenId, KeyType.Ascending).Option(o => { o.Name = "_asset_owner_tokenid_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

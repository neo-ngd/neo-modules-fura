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

        [BsonElement("timestamp")]
        public ulong Timestamp;

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

            await DB.Index<MarketModel>()
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Option(o => { o.Name = "_tokenid_amount_auctionType_asset_market_deadline_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_tokenid_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_market_amount_owner_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_market_owner_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_market_auctionType_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_amount_tokenid_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Option(o => { o.Name = "_auctionAsset_amount_auctionType_deadline_asset_tokenid_market_owner_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.TokenId, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Option(o => { o.Name = "_auctionType_deadline_asset_amount_tokenid_market_owner_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_auctionAsset_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Option(o => { o.Name = "_auctionAsset_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Option(o => { o.Name = "_market_asset_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.Auctor, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_auctionAsset_amount_auctor_auctionType_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.Auctor, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_auctionAsset_amount_auctor_auctionType_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Amount, KeyType.Ascending)
                .Key(a => a.Auctor, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.AuctionType, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Option(o => { o.Name = "_amount_auctor_auctionType_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
                .Key(a => a.Asset, KeyType.Ascending)
                .Key(a => a.AuctionAsset, KeyType.Ascending)
                .Key(a => a.Deadline, KeyType.Ascending)
                .Key(a => a.Owner, KeyType.Ascending)
                .Key(a => a.Market, KeyType.Ascending)
                .Key(a => a.Amount, KeyType.Ascending)
                .Option(o => { o.Name = "_asset_auctionAsset_deadline_owner_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Option(o => { o.Name = "_auctionAsset_deadline_owner_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Option(o => { o.Name = "_deadline_owner_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Option(o => { o.Name = "_owner_market_amount_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Asset, KeyType.Ascending)
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_asset_auctionAsset_auctor_bidAmount_amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_auctionAsset_auctor_bidAmount__amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_auctor_bidAmount_amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Asset, KeyType.Ascending)
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Bidder, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_asset_auctionAsset_bidder_bidAmount_amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Bidder, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_auctionAsset_bidder_bidAmount_amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Bidder, KeyType.Ascending)
               .Key(a => a.BidAmount, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_bidder_bidAmount_amount_deadline_owner_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Asset, KeyType.Ascending)
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_asset_auctionAsset_deadline_auctor_owner_amount_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.AuctionAsset, KeyType.Ascending)
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_auctionAsset_deadline_auctor_owner_amount_market_"; }).CreateAsync();

            await DB.Index<MarketModel>()
               .Key(a => a.Deadline, KeyType.Ascending)
               .Key(a => a.Auctor, KeyType.Ascending)
               .Key(a => a.Owner, KeyType.Ascending)
               .Key(a => a.Amount, KeyType.Ascending)
               .Key(a => a.Market, KeyType.Ascending)
               .Option(o => { o.Name = "_deadline_auctor_owner_amount_market_"; }).CreateAsync();

        }
    }
}

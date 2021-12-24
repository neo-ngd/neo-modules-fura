using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using System.Numerics;
using System.Threading.Tasks;

namespace Neo.Plugins.Models
{
    [Collection("Address-Asset")]
    public class AddressAssetModel : Entity
    {
        [UInt160AsString]
        [BsonElement("address")]
        public UInt160 Address { get; set; }

        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenID { get; set; }

        [BsonElement("balance")]
        public BsonDecimal128 Balance { get; set; }

        public AddressAssetModel()
        {

        }

        public AddressAssetModel(UInt160 address, UInt160 asset, BigInteger balance, string tokenid)
        {
            Address = address;
            Asset = asset;
            Balance = BsonDecimal128.Create(balance.ToString());
            TokenID = tokenid;
        }

        public static AddressAssetModel Get(UInt160 address, UInt160 asset, string tokenid)
        {
            AddressAssetModel addressAssetModel = DB.Find<AddressAssetModel>().Match(a => a.Address == address && a.Asset == asset && a.TokenID == tokenid).ExecuteFirstAsync().Result;
            return addressAssetModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<AddressAssetModel>(new CreateCollectionOptions<AddressAssetModel>());
            await DB.Index<AddressAssetModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.Address, KeyType.Ascending).Key(a => a.TokenID, KeyType.Ascending).Option(o => { o.Name = "_asset_address_tokenid_unique_"; o.Unique = true; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.Address, KeyType.Ascending).Option(o => { o.Name = "_asset_address_"; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Balance, KeyType.Ascending).Key(a => a.Asset, KeyType.Ascending).Option(o => { o.Name = "_balance_asset_"; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Balance, KeyType.Ascending).Key(a => a.Address, KeyType.Ascending).Option(o => { o.Name = "_balance_address_"; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Address, KeyType.Ascending).Option(o => { o.Name = "_address_"; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Balance, KeyType.Ascending).Option(o => { o.Name = "_balance_"; }).CreateAsync();
            await DB.Index<AddressAssetModel>().Key(a => a.Asset, KeyType.Ascending).Option(o => { o.Name = "_asset_"; }).CreateAsync();
        }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using System.Numerics;

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
    }
}

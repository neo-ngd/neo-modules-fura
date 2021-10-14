using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Nep11Properties")]
    public class Nep11PropertiesModel : Entity
    {
        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenId { get; set; }

        [BsonElement("properties")]
        public string Properties { get; set; }

        public Nep11PropertiesModel() { }

        public Nep11PropertiesModel(UInt160 asset, string tokenid, string properties)
        {
            Asset = asset;
            TokenId = tokenid;
            Properties = properties;
        }

        public static Nep11PropertiesModel Get(UInt160 asset, string tokenid)
        {
            Nep11PropertiesModel nep11PropertiesModel = DB.Find<Nep11PropertiesModel>().Match(a => a.Asset == asset && a.TokenId == tokenid).ExecuteFirstAsync().Result;
            return nep11PropertiesModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<Nep11PropertiesModel>(new CreateCollectionOptions<Nep11PropertiesModel>());
            await DB.Index<Nep11PropertiesModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.TokenId, KeyType.Ascending).Option(o => { o.Name = "_asset_tokenid_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

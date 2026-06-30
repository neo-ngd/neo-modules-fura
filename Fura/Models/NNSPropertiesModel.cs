using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("SelfControlNep11Properties")]
    public class NNSPropertiesModel : Entity
    {
        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenId { get; set; }

        [BsonElement("properties")]
        public string Properties { get; set; }

        public NNSPropertiesModel() { }

        public NNSPropertiesModel(UInt160 asset, string tokenid, string properties)
        {
            Asset = asset;
            TokenId = tokenid;
            Properties = properties;
        }

        public static NNSPropertiesModel Get(UInt160 asset, string tokenid)
        {
            NNSPropertiesModel nnsPropertiesModel = DB.Find<NNSPropertiesModel>().Match(a => a.Asset == asset && a.TokenId == tokenid).ExecuteFirstAsync().Result;
            return nnsPropertiesModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<NNSPropertiesModel>(o => { o = new CreateCollectionOptions<NNSPropertiesModel>(); });
            await DB.Index<NNSPropertiesModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.TokenId, KeyType.Ascending).Option(o => { o.Name = "_asset_tokenid_"; }).CreateAsync();
        }
    }
}

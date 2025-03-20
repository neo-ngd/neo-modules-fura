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
    public class MetaPropertiesModel : Entity
    {
        [UInt160AsString]
        [BsonElement("asset")]
        public UInt160 Asset { get; set; }

        [BsonElement("tokenid")]
        public string TokenId { get; set; }

        [BsonElement("properties")]
        public string Properties { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("series")]
        public string Series { get; set; }

        [BsonElement("supply")]
        public string Supply { get; set; }

        [BsonElement("thumbnail")]
        public string Thumbnail { get; set; }

        public MetaPropertiesModel() { }

        public MetaPropertiesModel(UInt160 asset, string tokenid, string properties)
        {
            Asset = asset;
            TokenId = tokenid;
            Properties = properties;
            try
            {
                Json.JObject jObject = (Json.JObject)Json.JObject.Parse(properties);
                Name = jObject["name"].GetString();
                Image = jObject["image"].GetString();
                Series = jObject["series"].GetString();
                Supply = jObject["supply"].GetString();
                Thumbnail = jObject["thumbnail"].GetString();
            }
            catch
            {

            }
        }

        public void UpdateProperties(string properties)
        {
            Properties = Properties;
            try
            {
                Json.JObject jObject = (Json.JObject)Json.JObject.Parse(properties);
                Name = jObject["name"].GetString();
                Image = jObject["image"].GetString();
                Series = jObject["series"].GetString();
                Supply = jObject["supply"].GetString();
                Thumbnail = jObject["thumbnail"].GetString();
            }
            catch
            {

            }
        }

        public static MetaPropertiesModel Get(UInt160 asset, string tokenid)
        {
            MetaPropertiesModel model = DB.Find<MetaPropertiesModel>().Match(a => a.Asset == asset && a.TokenId == tokenid).ExecuteFirstAsync().Result;
            return model;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<MetaPropertiesModel>(o => { o = new CreateCollectionOptions<MetaPropertiesModel>(); });
            await DB.Index<MetaPropertiesModel>().Key(a => a.Asset, KeyType.Ascending).Key(a => a.TokenId, KeyType.Ascending).Option(o => { o.Name = "_asset_tokenid_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

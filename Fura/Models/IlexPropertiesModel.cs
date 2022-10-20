using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;
using static System.Net.Mime.MediaTypeNames;

namespace Neo.Plugins.Models
{
    [Collection("SelfControlNep11Properties")]
    public class IlexPropertiesModel : Entity
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

        [BsonElement("tokenURI")]
        public string TokenURI { get; set; }

        public IlexPropertiesModel() { }

        public IlexPropertiesModel(UInt160 asset, string tokenid, string properties)
        {
            Asset = asset;
            TokenId = tokenid;
            Properties = properties;
            try
            {
                Json.JObject jObject = (Json.JObject)Json.JObject.Parse(properties);
                Name = jObject["name"].GetString();
                TokenURI = jObject["tokenURI"].GetString();
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
                TokenURI = jObject["tokenURI"].GetString();
            }
            catch
            {

            }
        }

        public static IlexPropertiesModel Get(UInt160 asset, string tokenid)
        {
            IlexPropertiesModel model = DB.Find<IlexPropertiesModel>().Match(a => a.Asset == asset && a.TokenId == tokenid).ExecuteFirstAsync().Result;
            return model;
        }

        public async static Task InitCollectionAndIndex()
        {
        }
    }
}

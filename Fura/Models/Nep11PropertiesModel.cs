using System;
using MongoDB.Bson.Serialization.Attributes;
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
    }
}

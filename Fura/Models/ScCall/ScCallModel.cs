using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("ScCall")]
    public class ScCallModel:Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt160AsString]
        [BsonElement("originSender")]
        public UInt160 OriginSender { get; set; }

        [UInt160AsString]
        [BsonElement("contractHash")]
        public UInt160 ContractHash { get; set; }

        [BsonElement("method")]
        public string Method { get; set; }

        [BsonElement("callFlags")]
        public string CallFlags { get; set; }

        [BsonElement("hexStringParams")]
        public string[] HexStringParams { get; set; }

        public ScCallModel(UInt256 txid,UInt160 originSender, UInt160 contractHash, string method, string callFlags, params string[] hexStringParams)
        {
            Txid = txid;
            OriginSender = originSender;
            ContractHash = contractHash;
            Method = method;
            CallFlags = callFlags;
            HexStringParams = hexStringParams;
        }

        public ScCallModel(ScCallModel scCallModel)
        {
            ID = scCallModel.ID;
            OriginSender = scCallModel.OriginSender;
            Txid = scCallModel.Txid;
            ContractHash = scCallModel.ContractHash;
            Method = scCallModel.Method;
            CallFlags = scCallModel.CallFlags;
            HexStringParams = scCallModel.HexStringParams;
        }
    }
}

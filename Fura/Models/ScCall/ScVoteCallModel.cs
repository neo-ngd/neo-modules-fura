using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("ScVoteCall")]
    public class ScVoteCallModel : Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [BsonElement("blockNumber")]
        public uint BlockNumber;

        [UInt160AsString]
        [BsonElement("voter")]
        public UInt160 Voter { get; set; }  //address

        [BsonElement("candidatePubKey")]
        public string CandidatePubKey { get; set; } //pubkey

        [UInt160AsString]
        [BsonElement("candidate")]
        public UInt160 Candidate { get; set; } //address

        public ScVoteCallModel(UInt256 txid, uint blockNumber, UInt160 voter, UInt160 candidate, string candidatePubKey)
        {
            Txid = txid;
            BlockNumber = blockNumber;
            Voter = voter;
            CandidatePubKey = candidatePubKey;
            Candidate = candidate;
        }
    }
}

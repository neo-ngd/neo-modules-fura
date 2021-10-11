using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
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

        public ScVoteCallModel() { }

        public ScVoteCallModel(UInt256 txid, uint blockNumber, UInt160 voter, UInt160 candidate, string candidatePubKey)
        {
            Txid = txid;
            BlockNumber = blockNumber;
            Voter = voter;
            CandidatePubKey = candidatePubKey;
            Candidate = candidate;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<ScVoteCallModel>(new CreateCollectionOptions<ScVoteCallModel>());
            await DB.Index<ScVoteCallModel>().Key(a => a.Voter, KeyType.Ascending).Option(o => { o.Name = "_voter_"; }).CreateAsync();
            await DB.Index<ScVoteCallModel>().Key(a => a.Txid, KeyType.Ascending).Option(o => { o.Name = "_txid_"; }).CreateAsync();
            await DB.Index<ScVoteCallModel>().Key(a => a.Candidate, KeyType.Ascending).Option(o => { o.Name = "_candidate_"; }).CreateAsync();
        }
    }
}

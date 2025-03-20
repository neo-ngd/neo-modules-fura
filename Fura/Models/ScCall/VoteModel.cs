using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Neo.Plugins.Models
{
    public enum Enum_VoteTrigger
    {
        Vote,
        Transfer
    }

    [Collection("Vote")]
    public class VoteModel : Entity
    {
        [UInt256AsString]
        [BsonElement("lastVoteTxid")]
        public UInt256 LastVoteTxid { get; set; }

        [BsonElement("blockNumber")]
        public uint BlockNumber { get; set; }

        [UInt160AsString]
        [BsonElement("voter")]
        public UInt160 Voter { get; set; }

        [UInt160AsString]
        [BsonElement("candidate")]
        public UInt160 Candidate { get; set; }

        [BsonElement("candidatePubKey")]
        public string CandidatePubKey { get; set; }

        [BsonElement("balanceOfVoter")]
        public BsonDecimal128 BalanceOfVoter { get; set; }

        [UInt256AsString]
        [BsonElement("lastTransferTxid")]
        public UInt256 LastTransferTxid { get; set; }

        public VoteModel() { }

        public VoteModel(UInt256 lastVoteTxid, uint blockNumber, UInt160 voter, UInt160 candidate, string candidatePubKey, string balanceOfVoter, UInt256 lastTransferTxid)
        {
            LastVoteTxid = lastVoteTxid;
            BlockNumber = blockNumber;
            Voter = voter;
            Candidate = candidate;
            CandidatePubKey = candidatePubKey;
            BalanceOfVoter = BsonDecimal128.Create(balanceOfVoter.WipeNumStrToFitDecimal128());
            LastTransferTxid = lastTransferTxid;
        }

        public static VoteModel Get(UInt160 voter)
        {
            VoteModel voteModel = DB.Find<VoteModel>().Match( v => v.Voter == voter ).ExecuteFirstAsync().Result;
            return voteModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<VoteModel>( o => { o = new CreateCollectionOptions<VoteModel>(); });
            await DB.Index<VoteModel>().Key(a => a.Voter, KeyType.Ascending).Option(o => { o.Name = "_voter_"; }).CreateAsync();
            await DB.Index<VoteModel>().Key(a => a.BlockNumber, KeyType.Ascending).Key(a => a.Voter, KeyType.Ascending).Option(o => { o.Name = "_blockNumber_voter_"; }).CreateAsync();
            await DB.Index<VoteModel>().Key(a => a.Candidate, KeyType.Ascending).Option(o => { o.Name = "_candidate_"; }).CreateAsync();
        }
    }
}

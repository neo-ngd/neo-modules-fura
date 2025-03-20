using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Candidate")]
    public class CandidateModel : Entity
    {
        [UInt160AsString]
        [BsonElement("candidate")]
        public UInt160 Candidate { get; set; }

        [BsonElement("state")]
        public bool State { get; set; }

        [BsonElement("votesOfCandidate")]
        public BsonDecimal128 VotesOfCandidate { get; set; }

        [BsonElement("isCommittee")]
        public bool IsCommittee { get; set; }

        public CandidateModel() { }

        public CandidateModel(UInt160 candidate, bool state, string votesOfCandidate, bool isCommittee = true)
        {
            Candidate = candidate;
            State = state;
            VotesOfCandidate = BsonDecimal128.Create(votesOfCandidate.WipeNumStrToFitDecimal128());
            IsCommittee = isCommittee;
        }

        public static CandidateModel Get(UInt160 candidate)
        {
            CandidateModel candidateModel = DB.Find<CandidateModel>().Match( c => c.Candidate == candidate).ExecuteFirstAsync().Result;
            return candidateModel;
        }

        public static List<CandidateModel> GetByIsCommittee(bool isCommittee)
        {
            List<CandidateModel> candidateModel = DB.Find<CandidateModel>().Match(c => c.IsCommittee == isCommittee).ExecuteAsync().Result;
            return candidateModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<CandidateModel>( o => { o = new CreateCollectionOptions<CandidateModel>(); });
            await DB.Index<CandidateModel>().Key(a => a.Candidate, KeyType.Ascending).Option(o => { o.Name = "_candidate_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

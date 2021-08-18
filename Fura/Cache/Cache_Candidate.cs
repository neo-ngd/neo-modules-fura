using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheCandidateParams
    {
        public UInt160 Candidate;
        public string CandidatePubKey;
        public bool State;
    }

    public class CacheCandidate : IDBCache
    {
        private ConcurrentDictionary<UInt160, CacheCandidateParams> D_Candidate;
        private ConcurrentDictionary<UInt160, CandidateModel> D_CandidateModel;

        public CacheCandidate()
        {
            D_CandidateModel = new ConcurrentDictionary<UInt160, CandidateModel>();
            D_Candidate = new ConcurrentDictionary<UInt160, CacheCandidateParams>();
        }

        public void Clear()
        {
            D_CandidateModel = new ConcurrentDictionary<UInt160, CandidateModel>();
            D_Candidate = new ConcurrentDictionary<UInt160, CacheCandidateParams>();
        }

        public void AddNeedUpdate(UInt160 candidate, string candidatePubKey, bool state)
        {
            if (candidate is null)
                return;
            D_Candidate[candidate] = new() { Candidate = candidate, State = state , CandidatePubKey  = candidatePubKey};
        }

        public List<CacheCandidateParams> GetNeedUpdate()
        {
            return D_Candidate.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheCandidateParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                BigInteger votes = Neo.Plugins.VM.Helper.GetCandidateVotes(list[i].CandidatePubKey, system, snapshot);
                AddOrUpdate(list[i].Candidate, list[i].State, votes);
            });
        }

        public CandidateModel Get(UInt160 candidate)
        {
            if (D_CandidateModel.ContainsKey(candidate))
            {
                return D_CandidateModel[candidate];
            }
            else
            {
                var candidateModel = CandidateModel.Get(candidate);
                return candidateModel;
            }
        }

        public void AddOrUpdate(UInt160 candidate, bool state, BigInteger votes)
        {
            if (candidate is null || candidate == UInt160.Zero) return;
            CandidateModel candidateModel = Get(candidate);
            if (candidateModel is null)
            {
                candidateModel = new CandidateModel(candidate, state, votes.ToString(), false);
            }
            else
            {
                candidateModel.State = state;
                candidateModel.VotesOfCandidate = Decimal128.Parse(votes.ToString());
            }
            D_CandidateModel[candidate] = candidateModel;
        }

        public void Save(Transaction tran)
        {
            if (D_CandidateModel.Values.Count > 0)
                tran.SaveAsync(D_CandidateModel.Values).Wait();
        }
    }
}

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
        public EnumCandidateState State;
    }

    public enum EnumCandidateState
    {
        Unknow,
        True,
        False
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

        public void AddNeedUpdate(UInt160 candidate, string candidatePubKey, EnumCandidateState state)
        {
            if (candidate is null)
                return;
            if (D_Candidate.ContainsKey(candidate) && state == EnumCandidateState.Unknow)
                return;
            D_Candidate[candidate] = new() { Candidate = candidate, State = state, CandidatePubKey = candidatePubKey };
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

        public void AddOrUpdate(UInt160 candidate, EnumCandidateState state, BigInteger votes)
        {
            if (candidate is null || candidate == UInt160.Zero) return;
            CandidateModel candidateModel = Get(candidate);
            if (candidateModel is null)
            {
                //如果之前candidate不存在，那么unkown意味着false
                var _state = state == EnumCandidateState.True ? true : false;
                candidateModel = new CandidateModel(candidate, _state, votes.ToString(), false);
            }
            else
            {
                //如果之前candidate存在，那么unkown意味着不变
                if(state != EnumCandidateState.Unknow)
                {
                    candidateModel.State = state == EnumCandidateState.True ? true : false;
                }
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

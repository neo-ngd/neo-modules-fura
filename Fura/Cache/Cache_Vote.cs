using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheVoteParams
    {
        public UInt160 Voter;
        public uint BlockNumber;
        public UInt256 VoteTxid;
        public UInt256 TranTxid;
        public UInt160 Candidate;
        public string CandidatePubKey;
    }

    public class CacheVote : IDBCache
    {
        private ConcurrentDictionary<UInt160, CacheVoteParams> D_Vote;
        private ConcurrentDictionary<UInt160, VoteModel> D_VoteModel;
        private ConcurrentDictionary<UInt160, VoteModel> D_VoteModel_Read;

        public CacheVote()
        {
            D_VoteModel = new ConcurrentDictionary<UInt160, VoteModel>();
            D_Vote = new ConcurrentDictionary<UInt160, CacheVoteParams>();
            D_VoteModel_Read = new ConcurrentDictionary<UInt160, VoteModel>();
        }

        public void Clear()
        {
            D_VoteModel.Clear();
            D_Vote.Clear();
            D_VoteModel_Read.Clear();
        }

        public void AddNeedUpdate(UInt160 voter, UInt256 voteTxid, UInt256 tranTxid, uint blockNumber, UInt160 candidate, string candidatePubKey)
        {
            D_Vote[voter] = new CacheVoteParams() { Voter = voter, VoteTxid = voteTxid, TranTxid = tranTxid, BlockNumber = blockNumber , Candidate  = candidate, CandidatePubKey = candidatePubKey };
        }

        public List<CacheVoteParams> GetNeedUpdate()
        {
            return D_Vote.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheVoteParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                BigInteger balanceOfVoter = Neo.Plugins.VM.Helper.GetNeoBalanceOf(list[i].Voter, system, snapshot);
                AddOrUpdate(list[i].VoteTxid, list[i].BlockNumber, list[i].Voter, list[i].Candidate, list[i].CandidatePubKey, balanceOfVoter.ToString(), list[i].TranTxid);
            });
        }

        public VoteModel Get(UInt160 address)
        {
            if (D_VoteModel.ContainsKey(address))
            {
                return D_VoteModel[address];
            }
            else if (D_VoteModel_Read.ContainsKey(address))
            {
                return D_VoteModel_Read[address];
            }
            else
            {
                D_VoteModel_Read[address] = VoteModel.Get(address);
                return D_VoteModel_Read[address];
            }
        }

        public void AddOrUpdate(UInt256 lastVoteTxid, uint blockNumber, UInt160 voter, UInt160 candidate, string candidatePubKey, string balanceOfVoter, UInt256 lastTranTxid)
        {
            VoteModel voteModel = Get(voter);
            if (voteModel is null)
            {
                voteModel = new VoteModel(lastVoteTxid, blockNumber, voter, candidate, candidatePubKey, balanceOfVoter, lastTranTxid);
            }
            else
            {
                voteModel.LastVoteTxid = lastVoteTxid ?? voteModel.LastVoteTxid;
                voteModel.BlockNumber = blockNumber;
                voteModel.Candidate = candidate ?? voteModel.Candidate;
                voteModel.CandidatePubKey = candidatePubKey ?? voteModel.CandidatePubKey;
                voteModel.BalanceOfVoter = Decimal128.Parse(balanceOfVoter);
                voteModel.LastTransferTxid = lastTranTxid ?? voteModel.LastTransferTxid;
            }
            D_VoteModel[voter] = voteModel;
        }

        public void Save(Transaction tran)
        {
            if (D_VoteModel.Values.Count > 0)
                tran.SaveAsync(D_VoteModel.Values).Wait();
        }
    }
}

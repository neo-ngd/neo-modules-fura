using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Models;
using Neo.SmartContract;
using System.Linq;
using MongoDB.Bson;
using Neo.Plugins.Cache;
using System.Threading.Tasks;

namespace Neo.Plugins
{
    public class ScCallMgr
    {
        Dictionary<UInt256, Func<ScCallModel, NeoSystem, Block, DataCache, bool>> dic_filter = new Dictionary<UInt256, Func<ScCallModel, NeoSystem, Block, DataCache, bool>>();

        private static ScCallMgr ins;

        private static object lockObj = new object();

        public static ScCallMgr Ins
        {
            get
            {
                if (ins is null)
                {
                    lock (lockObj)
                    {
                        if (ins is null)
                            ins = new ScCallMgr();
                    }
                }
                return ins;
            }

        }

        public ScCallMgr()
        {
            Register(Neo.SmartContract.Native.NativeContract.NEO.Hash,"vote", ExecuteScVoteCall);
            Register(Neo.SmartContract.Native.NativeContract.NEO.Hash, "registerCandidate", ExecuteRegisterCandidate);
            Register(Neo.SmartContract.Native.NativeContract.NEO.Hash, "unregisterCandidate", ExecuteUnRegisterCandidate);
        }

        public void Register(UInt160 contractHash,string method, Func<ScCallModel, NeoSystem, Block, DataCache, bool> entity)
        {
            dic_filter.Add(GetKey(contractHash, method), entity);
        }

        public UInt256 GetKey(UInt160 contractHash, string method)
        {
            return new UInt256((UTF8Encoding.UTF8.GetBytes(contractHash + method)).Sha256());
        }

        public void Filter(List<ScCallModel> scCallModels, NeoSystem system, Block block, DataCache snapshot)
        {
            //foreach(var sc in scCallModels)
            //{
            //    var key = GetKey(sc.ContractHash,sc.Method);
            //    if (dic_filter.ContainsKey(key))
            //    {
            //        dic_filter[key](sc, system, block, snapshot);
            //    }
            //}
            Parallel.For(0, scCallModels.Count, (i) =>
            {
                 var sc = scCallModels[i];
                 var key = GetKey(sc.ContractHash, sc.Method);
                 if (dic_filter.ContainsKey(key))
                 {
                     dic_filter[key](sc, system, block, snapshot);
                 }
            });
        }

        public bool ExecuteScVoteCall(ScCallModel scCall, NeoSystem system, Block block, DataCache snapshot)
        {
            if (scCall.HexStringParams.Length != 2) return false;

            //增加scvotecall记录进cache
            UInt160 voter = null;
            UInt160 candidate = null;
            bool succ = UInt160.TryParse(scCall.HexStringParams[0].HexToBytes().Reverse().ToArray().ToHexString(), out voter);
            if (!succ) return false;
            if (scCall.HexStringParams[1] != string.Empty)
            {
                ECPoint ecPoint = null;
                succ = ECPoint.TryParse(scCall.HexStringParams[1], ECCurve.Secp256r1, out ecPoint);
                if (!succ) return false;
                candidate = Contract.CreateSignatureContract(ecPoint).ScriptHash;
            }
            string candidatePubKey = scCall.HexStringParams[1];
            var scVoteCallModel = DBCache.Ins.cacheScVoteCall.Add(scCall.Txid, block.Index, voter, candidate, candidatePubKey);

            //哪些voter需要更新记录
            DBCache.Ins.cacheVote.AddNeedUpdate(voter, scCall.Txid, null, block.Index, candidate, candidatePubKey);
            //哪些candidate需要更新记录
            DBCache.Ins.cacheCandidate.AddNeedUpdate(candidate, scCall.HexStringParams[1], true);
            if (DBCache.Ins.cacheVote.Get(voter) is not null)
            {
                DBCache.Ins.cacheCandidate.AddNeedUpdate(scVoteCallModel.Candidate, scCall.HexStringParams[1], true);
            }
            return true;
        }

        public bool ExecuteRegisterCandidate(ScCallModel scCall, NeoSystem system, Block block, DataCache snapshot)
        {
            if (scCall.HexStringParams.Length != 1) return false;
            UInt160 candidate = Contract.CreateSignatureContract(ECPoint.Parse(scCall.HexStringParams[0], ECCurve.Secp256r1)).ScriptHash;
            //哪些candidate需要更新记录
            DBCache.Ins.cacheCandidate.AddNeedUpdate(candidate, scCall.HexStringParams[0], true);
            return true;
        }

        public bool ExecuteUnRegisterCandidate(ScCallModel scCall, NeoSystem system, Block block, DataCache snapshot)
        {
            if (scCall.HexStringParams.Length != 1) return false;
            UInt160 candidate = Contract.CreateSignatureContract(ECPoint.Parse(scCall.HexStringParams[0], ECCurve.Secp256r1)).ScriptHash;
            //哪些candidate需要更新记录
            DBCache.Ins.cacheCandidate.AddNeedUpdate(candidate, scCall.HexStringParams[0], false);
            return true;
        }
    }
}

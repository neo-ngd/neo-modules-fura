using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Entities;
using Neo.Cryptography.ECC;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Models;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.Plugins.Cache;
using System.Threading.Tasks;

namespace Neo.Plugins
{
    public class Fura : Plugin, IPersistencePlugin
    {
        public override string Name => "Fura";

        public override string Description => "Analyze the data and store it in MongoDB";

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            if (Settings.Default.ConnectionString != "")
            {
                MongoClient.InitDB(Settings.Default.DbName, Settings.Default.ConnectionString).Wait();
            }
            else
            {
                MongoClient.InitDB(Settings.Default.DbName, Settings.Default.Host, Settings.Default.Port, Settings.Default.User, Settings.Default.Password).Wait();
            }
            Loger.Common("mongodb init succ");
            AssetModel assetModel = (from a in DB.Queryable<AssetModel>() where a.Hash.Equals(NativeContract.GAS.Hash) select a).FirstOrDefault();
            if (assetModel is not null)
                return;

            using (var transaction = new MongoDB.Entities.Transaction())
            {
                ContractModel contractModel_oracle = new(NativeContract.Oracle.Hash, "Oracle", NativeContract.Oracle.Id, 0, NativeContract.Oracle.Nef.ToJson(), NativeContract.Oracle.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_roleManagement = new(NativeContract.RoleManagement.Hash, "RoleManagement", NativeContract.RoleManagement.Id, 0, NativeContract.RoleManagement.Nef.ToJson(), NativeContract.RoleManagement.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_policy = new(NativeContract.Policy.Hash, "Policy", NativeContract.Policy.Id, 0, NativeContract.Policy.Nef.ToJson(), NativeContract.Policy.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_gas = new(NativeContract.GAS.Hash, "GasToken", NativeContract.GAS.Id, 0, NativeContract.GAS.Nef.ToJson(), NativeContract.GAS.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_neo = new(NativeContract.NEO.Hash, "NeoToken", NativeContract.NEO.Id, 0, NativeContract.NEO.Nef.ToJson(), NativeContract.NEO.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_ledger = new(NativeContract.Ledger.Hash, "Ledger", NativeContract.Ledger.Id, 0, NativeContract.Ledger.Nef.ToJson(), NativeContract.Ledger.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_cryptoLib = new(NativeContract.CryptoLib.Hash, "CryptoLib", NativeContract.CryptoLib.Id, 0, NativeContract.CryptoLib.Nef.ToJson(), NativeContract.CryptoLib.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_stdLib = new(NativeContract.StdLib.Hash, "StdLib", NativeContract.StdLib.Id, 0, NativeContract.StdLib.Nef.ToJson(), NativeContract.StdLib.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_contractManagement = new(NativeContract.ContractManagement.Hash, "ContractManagement", NativeContract.ContractManagement.Id, 0, NativeContract.ContractManagement.Nef.ToJson(), NativeContract.ContractManagement.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                transaction.SaveAsync(contractModel_oracle).Wait();
                transaction.SaveAsync(contractModel_roleManagement).Wait();
                transaction.SaveAsync(contractModel_policy).Wait();
                transaction.SaveAsync(contractModel_gas).Wait();
                transaction.SaveAsync(contractModel_neo).Wait();
                transaction.SaveAsync(contractModel_ledger).Wait();
                transaction.SaveAsync(contractModel_cryptoLib).Wait();
                transaction.SaveAsync(contractModel_stdLib).Wait();
                transaction.SaveAsync(contractModel_contractManagement).Wait();
                AssetModel assetModel_gas = new(NativeContract.GAS.Hash, system.GenesisBlock.Timestamp, "GasToken", 8, "GAS", 0, EnumAssetType.NEP17);
                AssetModel assetModel_neo = new(NativeContract.NEO.Hash, system.GenesisBlock.Timestamp, "NeoToken", 0, "NEO", 0, EnumAssetType.NEP17);
                transaction.SaveAsync(assetModel_gas).Wait();
                transaction.SaveAsync(assetModel_neo).Wait();
                transaction.CommitAsync().Wait();
            }
            Loger.Common("data init succ");
        }

        void IPersistencePlugin.OnPersist(NeoSystem system, Block block, DataCache snapshot, IReadOnlyList<Blockchain.ApplicationExecuted> applicationExecutedList)
        {
            bool loop = true;
            uint errLoopTimes = 0;
            while (loop)
            {
                var state = RegisterPersistInsert(block.Index);
                switch (state)
                {
                    case EnumRecordState.Confirm:
                        Loger.Common(string.Format("OnPersist------ {0} 高度的数据已经被录入", block.Index));
                        loop = false;
                        break;
                    case EnumRecordState.None:
                        var time0 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                        using (var transaction = DB.Transaction())
                        {
                            try
                            {
                                //先查询这个高度有没有存过execution，如果存过了，证明是断点重连，需要排重。
                                bool exsit = ExecutionModel.ExistByBlockHash(block.Hash);
                                DBCache.Ins.Reset();
                                if (!exsit)
                                {
                                    Parallel.For(0, applicationExecutedList.Count, (i) =>
                                    {
                                        ExecApplicationExecuted(applicationExecutedList[i], system, block, snapshot);
                                    });
                                }
                                //标记此块已经结束
                                RecordPersistModel recordPersistModel = RecordPersistModel.Get(block.Index);
                                recordPersistModel.State = EnumRecordState.Confirm.ToString();
                                transaction.SaveAsync(recordPersistModel).Wait();
                                DBCache.Ins.Save(system, snapshot, transaction);
                                transaction.CommitAsync().Wait();
                                loop = false;
                                var time1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                                Loger.Common(String.Format("OnPersist------ {0} 高度的数据录入耗时 {1} ms", block.Index, time1 - time0));
                            }
                            catch (Exception e)
                            {
                                transaction.AbortAsync().Wait();
                                RecordPersistModel.Delete(block.Index);
                                errLoopTimes++;
                                DebugModel debugModel = new(string.Format("{0}---ExecApplicationExecuted----block: {1}, loopTimes:{2}, error: {3}", Settings.Default.PName, block.Index, errLoopTimes, e));
                                debugModel.SaveAsync().Wait();
                                if (errLoopTimes < 10)
                                {
                                    System.Threading.Thread.Sleep(Settings.Default.SleepTime * 100);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }

                        break;
                    case EnumRecordState.Pending:
                    default:
                        Loger.Common(string.Format("OnPersist------ {0} 高度的数据正在被录入", block.Index));
                        System.Threading.Thread.Sleep(Settings.Default.SleepTime);
                        continue;
                }
            }
        }

        void IPersistencePlugin.OnCommit(NeoSystem system, Block block, DataCache snapshot)
        {
            bool loop = true;
            uint errLoopTimes = 0;
            while (loop)
            {
                var state = RegisterCommitInsert(block.Index);
                switch (state)
                {
                    case EnumRecordState.Confirm:
                        Loger.Common(string.Format("OnCommit------ {0} 高度的数据已经被录入", block.Index));
                        loop = false;
                        break;
                    case EnumRecordState.None:
                        var time0 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                        using (var transaction = DB.Transaction())
                        {
                            try
                            {
                                ExecBlock(transaction, system, block, snapshot);
                                var time1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                                Loger.Common(string.Format("OnCommit------ {0} 高度的数据录入耗时 {1} ms", block.Index, time1 - time0));
                                loop = false;
                            }
                            catch (Exception e)
                            {
                                transaction.AbortAsync().Wait();
                                RecordCommitModel.Delete(block.Index);
                                errLoopTimes++;
                                DebugModel debugModel = new(string.Format("{0}---ExecBlock----block: {1}, loopTimes:{2}, error: {3}", Settings.Default.PName, block.Index, errLoopTimes, e));
                                debugModel.SaveAsync().Wait();
                                if (errLoopTimes < 10)
                                {
                                    System.Threading.Thread.Sleep(Settings.Default.SleepTime * 100);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }

                        break;
                    case EnumRecordState.Pending:
                    default:
                        Loger.Common(string.Format("OnCommit------ {0} 高度的数据正在被录入", block.Index));
                        System.Threading.Thread.Sleep(Settings.Default.SleepTime);
                        continue;
                }
            }
        }

        /// <summary>
        /// 申请处理这个高度的persist
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        EnumRecordState RegisterPersistInsert(uint blockIndex)
        {
            try
            {
                long curTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000; //s
                RecordPersistModel recordPersistModel = RecordPersistModel.Get(blockIndex);
                if (recordPersistModel is null)
                {
                    recordPersistModel = new RecordPersistModel() { BlockIndex = blockIndex, State = EnumRecordState.Pending.ToString(), PName = Settings.Default.PName, Timestamp = curTime };
                    DB.SaveAsync(recordPersistModel).Wait();
                    return EnumRecordState.None;
                }
                else if (recordPersistModel.State == EnumRecordState.Confirm.ToString())
                {
                    return EnumRecordState.Confirm;
                }
                else if (recordPersistModel.State == EnumRecordState.Pending.ToString() && recordPersistModel.Timestamp + Settings.Default.WaitTime < curTime)
                {
                    RecordPersistModel.Delete(blockIndex);
                    return EnumRecordState.Pending;
                }
                else if (recordPersistModel.PName == Settings.Default.PName)
                {
                    return EnumRecordState.None;
                }
            }
            catch(Exception e)
            {
                Loger.Warning(string.Format("Persist:{0}", e.Message));
            }
            return EnumRecordState.Pending;
        }

        EnumRecordState RegisterCommitInsert(uint blockIndex)
        {
            try
            {
                long curTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000; //s
                RecordCommitModel recordCommitModel = RecordCommitModel.Get(blockIndex);
                if (recordCommitModel is null)
                {
                    recordCommitModel = new RecordCommitModel() { BlockIndex = blockIndex, State = EnumRecordState.Pending.ToString(), PName = Settings.Default.PName, Timestamp = curTime };
                    DB.SaveAsync(recordCommitModel).Wait();
                    return EnumRecordState.None;
                }
                else if (recordCommitModel.State == EnumRecordState.Confirm.ToString())
                {
                    return EnumRecordState.Confirm;
                } 
                else if (recordCommitModel.State == EnumRecordState.Pending.ToString() && recordCommitModel.Timestamp + Settings.Default.WaitTime < curTime)
                {
                    RecordCommitModel.Delete(blockIndex);
                    return EnumRecordState.Pending;
                }
                else if (recordCommitModel.PName == Settings.Default.PName)
                {
                    return EnumRecordState.None;
                }
            }
            catch(Exception e)
            {
                Loger.Warning(string.Format("Commit:{0}", e.Message));
            }
            return EnumRecordState.Pending;
        }

        void ExecApplicationExecuted(Blockchain.ApplicationExecuted applicationExecuted,NeoSystem system, Block block, DataCache snapshot)
        {
            ExecutionModel executionModel;
            List<ScCallModel> list_ScCall = new List<ScCallModel>();
            if (applicationExecuted.Transaction is not null)
            {
                executionModel = new ExecutionModel(applicationExecuted.Transaction.Hash, block.Hash, block.Timestamp, applicationExecuted.Trigger.ToString(), applicationExecuted.VMState.ToString(), applicationExecuted.Exception?.ToString(), applicationExecuted.GasConsumed); ;
                //通过解析script得到调用了哪些合约哪些方法，从而处理一些特殊数据
                list_ScCall = VM.Helper.Script2ScCallModels(applicationExecuted.Transaction.Script, applicationExecuted.Transaction.Hash, applicationExecuted.Transaction.Sender);
            }
            else
            {
                executionModel = new ExecutionModel(UInt256.Zero, block.Hash, block.Timestamp, applicationExecuted.Trigger.ToString(), applicationExecuted.VMState.ToString(), applicationExecuted.Exception?.ToString(), applicationExecuted.GasConsumed);
            }
            //处理script解析出来的合约调用信息
            if (list_ScCall.Count > 0)
            {
                DBCache.Ins.cacheScCall.Add(list_ScCall);
                ScCallMgr.Ins.Filter(list_ScCall, system, block, snapshot);
            }
            DBCache.Ins.cacheExecution.Add(executionModel);
            //处理event分析出来的信息
            List<NotificationModel> notificationModels = new List<NotificationModel>();
            for (var i = 0; i < applicationExecuted.Notifications.Length; i++)
            {
                var n = applicationExecuted.Notifications[i];
                var notificationModel = new NotificationModel(executionModel.Txid, i, executionModel.BlockHash, executionModel.Timestamp, n.ScriptHash, n.EventName, executionModel.VmState, n.State);
                notificationModels.Add(notificationModel);
            }
            if (notificationModels.Count > 0)
            {
                DBCache.Ins.cacheNotification.Add(notificationModels);
                NotificationMgr.Ins.Filter(notificationModels, system, block, snapshot);
            }
        }

        void ExecBlock(MongoDB.Entities.Transaction transaction, NeoSystem system, Block block, DataCache snapshot)
        {
            //如果这个高度的block数据能在数据库中查到，证明已经录入过了（防止重启重录）
            BlockModel blockModel = BlockModel.Get(block.Hash);
            if (blockModel is not null)
            {
                return;
            }

            BlockModel bm = new(block);
            HeaderModel hm = new(block.Header);
            long sysFee = 0;
            long netFee = 0;
            TransactionModel[] tms = new TransactionModel[block.Transactions.Length];
            for (var i = 0; i < block.Transactions.Length; i++)
            {
                var t = block.Transactions[i];
                sysFee += t.SystemFee;
                netFee += t.NetworkFee;
                TransactionModel tm = new(t, block.Hash, block.Timestamp, block.Index);
                tms[i] = tm;
            }
            bm.SystemFee = sysFee;
            bm.NetworkFee = netFee;
            transaction.SaveAsync(bm).Wait();
            transaction.SaveAsync(hm).Wait();
            if (tms.Length > 0)
            {
                transaction.SaveAsync(tms).Wait();
            }
            //每隔一定的块更新committee表
            if (block.Index % system.Settings.CommitteeMembersCount == 0)
            {
                Loger.Common(string.Format("blockindex:{0},更新committee", block.Index));
                //先把上一批的重置了
                List<CandidateModel> candidateModel_old = CandidateModel.GetByIsCommittee(true);
                if (candidateModel_old.Count > 0)
                {
                    candidateModel_old = candidateModel_old.Select(c => { c.IsCommittee = false; return c; }).ToList();
                    transaction.SaveAsync(candidateModel_old).Wait();
                }
                //更新新的一批次
                ECPoint[] committees = Neo.SmartContract.Native.NeoToken.NEO.GetCommittee(snapshot);
                CandidateModel[] candidateModels_new = committees.Select(c =>
                {
                    var hash = Contract.CreateSignatureContract(c).ScriptHash;
                    var candidateModel = CandidateModel.Get(hash);
                    if (candidateModel is null)
                    {
                        var votes = Neo.Plugins.VM.Helper.GetCandidateVotes(c, system, snapshot);
                        candidateModel = new CandidateModel(hash, true, votes.ToString(), true);
                    }
                    candidateModel.IsCommittee = true;
                    return candidateModel;
                }).ToArray();
                transaction.SaveAsync(candidateModels_new).Wait();
            }

            //将此块的record标记为完成
            RecordCommitModel recordCommitModel = RecordCommitModel.Get(block.Index);
            recordCommitModel.State = EnumRecordState.Confirm.ToString();
            transaction.SaveAsync(recordCommitModel).Wait();

            transaction.CommitAsync().Wait();

        }
    }
}

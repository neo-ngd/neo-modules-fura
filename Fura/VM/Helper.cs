using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using MongoDB.Entities;
using Neo.Cryptography.ECC;
using Neo.Persistence;
using Neo.Plugins.Models;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.Plugins.VM
{
    internal class CandidateState: IInteroperable
    {
        public bool Registered = true;
        public BigInteger Votes;

        public void FromStackItem(StackItem stackItem)
        {
            Struct @struct = (Struct)stackItem;
            Registered = @struct[0].GetBoolean();
            Votes = @struct[1].GetInteger();
        }

        public StackItem ToStackItem(ReferenceCounter referenceCounter)
        {
            return new Struct(referenceCounter) { Registered, Votes };
        }
    }


    public static class Helper
    {
        public static List<ScCallModel> Script2ScCallModels(byte[] script, UInt256 txid, UInt160 sender)
        {
            List<ScCallModel> scCalls = new List<ScCallModel>();
            List<Instruction> instructions = Script2Instruction(script).ToArray().Reverse().ToList(); ;
            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode == Neo.VM.OpCode.SYSCALL && new BigInteger(instruction.Operand.Span.ToArray()) == ApplicationEngine.System_Contract_Call)
                {
                    //下一个是调用的合约hash
                    var contractHash = instructions[++index].Operand.Span.ToArray().Reverse().ToArray().ToHexString();
                    //再下一个是方法名
                    string method = Encoding.UTF8.GetString(instructions[++index].Operand.Span.ToArray());
                    //再下一个是callflags
                    CallFlags callFlags = Opcode2CallFlags(instructions[++index].OpCode);
                    int paramsCount = 0;
                    if (instructions[index + 1].OpCode != Neo.VM.OpCode.NEWARRAY0)
                    {
                        //再下一个是pack
                        ++index;
                        //再下一个是参数的数量
                        var ins = instructions[++index];
                        paramsCount = Opcode2PushNumber(ins.OpCode, ins.Operand.Span.ToArray());
                    }
                    //接下来获取参数
                    string[] hexParams = new string[paramsCount];
                    for (var i = 0; i < paramsCount; i++)
                    {
                        hexParams[i] = instructions[++index].Operand.Span.ToHexString();
                    }
                    scCalls.Add(new(txid, sender, UInt160.Parse(contractHash), method, callFlags.ToString(), hexParams));
                }
            }
            return scCalls;
        }

        public static List<Instruction> Script2Instruction(byte[] script)
        {
            List<Instruction> instructions = new List<Instruction>();
            Script s = new Script(script,true);
            for (int ip = 0; ip < s.Length; ip += s.GetInstruction(ip).Size)
            {
                var instruction = s.GetInstruction(ip);
                instructions.Add(instruction);
            }
            return instructions;
        }

        public static CallFlags Opcode2CallFlags(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.PUSH0:
                    return CallFlags.None;
                case OpCode.PUSH1:
                    return CallFlags.ReadStates;
                case OpCode.PUSH2:
                    return CallFlags.WriteStates;
                case OpCode.PUSH4:
                    return CallFlags.AllowCall;
                case OpCode.PUSH8:
                    return CallFlags.AllowNotify;
                case OpCode.PUSH3:
                    return CallFlags.States;
                case OpCode.PUSH5:
                    return CallFlags.ReadOnly;
                case OpCode.PUSH15:
                    return CallFlags.All;
                default:
                    throw new Exception("unhandle opcode");

            }
        }

        public static int Opcode2PushNumber(OpCode opCode, byte[] bytes)
        {
            switch (opCode)
            {
                case OpCode.PUSH0:
                case OpCode.PUSH1:
                case OpCode.PUSH2:
                case OpCode.PUSH3:
                case OpCode.PUSH4:
                case OpCode.PUSH5:
                case OpCode.PUSH6:
                case OpCode.PUSH7:
                case OpCode.PUSH8:
                case OpCode.PUSH9:
                case OpCode.PUSH10:
                case OpCode.PUSH11:
                case OpCode.PUSH12:
                case OpCode.PUSH13:
                case OpCode.PUSH14:
                case OpCode.PUSH15:
                case OpCode.PUSH16:
                    return int.Parse(opCode.ToString().Substring(4));
                case OpCode.PUSHM1:
                    return -1;
                case OpCode.PUSHINT8:
                    return bytes[0];
                case OpCode.PUSHINT16:
                    return BitConverter.ToInt16(bytes);
                case OpCode.PUSHINT32:
                    return BitConverter.ToInt32(bytes);
                default:
                    throw new Exception("unhandle opcode:" + opCode.ToString());

            }
        }

        public static BigInteger GetNeoBalanceOf(UInt160 user, NeoSystem system, DataCache snapshot)
        {
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitDynamicCall(Neo.SmartContract.Native.NativeContract.NEO.Hash, "balanceOf", user);
                script = sb.ToArray();
            }

            using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
            {
                if (engine.State.HasFlag(VMState.FAULT))
                {
                    Console.WriteLine("GetNeoBalanceOf error");
                    return 0;
                }
                return engine.ResultStack.Pop().GetInteger();
            }
        }

        public static BigInteger GetCandidateVotes(string candidate, NeoSystem system, DataCache snapshot)
        {
            if (candidate == string.Empty || candidate is null) return 0;
            ECPoint ecpoint = ECPoint.Parse(candidate, ECCurve.Secp256r1);
            StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.NEO.Id, 33).Add(ecpoint);
            try
            {
                CandidateState state = snapshot.GetAndChange(key)?.GetInteroperable<CandidateState>();
                return state?.Votes ?? 0;
            }
            catch
            {
                DebugModel debugModel = new(string.Format("GetCandidateVotes----candidate: {0}", candidate));
                debugModel.SaveAsync().Wait();
                return 0;
            }

        }

        public static BigInteger GetCandidateVotes(ECPoint ecpoint, NeoSystem system, DataCache snapshot)
        {
            StorageKey key = new KeyBuilder(Neo.SmartContract.Native.NativeContract.NEO.Id, 33).Add(ecpoint);
            try
            {
                CandidateState state = snapshot.GetAndChange(key)?.GetInteroperable<CandidateState>();
                return state?.Votes ?? 0;
            }
            catch
            {
                DebugModel debugModel = new(string.Format("GetCandidateVotes----candidate: {0}", ecpoint));
                debugModel.SaveAsync().Wait();
                return 0;
            }
        }

        /// <summary>
        /// 获取资产的信息
        /// </summary>
        /// <param name="system"></param>
        /// <param name="snapshot"></param>
        /// <param name="asset"></param>
        /// <param name="onlyTotalSupply"></param>
        /// <returns>（symbol,decimals,totalsupply）</returns>
        public static (string, byte, BigInteger) GetAssetInfo(NeoSystem system, DataCache snapshot, UInt160 asset)
        {
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitDynamicCall(asset, "symbol");
                sb.EmitDynamicCall(asset, "decimals");
                script = sb.ToArray();
            }
            (string, byte, BigInteger) t = new("", 0, 0);
            using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
            {
                if (engine.State.HasFlag(VMState.FAULT))
                {
                    Console.WriteLine("Error:GetNep17Info,VMState.FAULT" + asset.ToString());
                    return t;
                }
                t.Item2 = (byte)engine.ResultStack.Pop().GetInteger();
                t.Item1 = engine.ResultStack.Pop().GetString();
            }
            t.Item3 = GetAssetTotalSupply(system, snapshot, asset);
            return t;
        }

        public static BigInteger GetAssetTotalSupply(NeoSystem system, DataCache snapshot, UInt160 asset)
        {
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitDynamicCall(asset, "totalSupply");
                script = sb.ToArray();
            }
            BigInteger totalSupply = 0;
            using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
            {
                if (engine.State.HasFlag(VMState.FAULT))
                {
                    Console.WriteLine("Error:GetNep11Info,VMState.FAULT" + asset.ToString()); //后面需要去掉，返回null
                    return 0;
                }
                totalSupply = engine.ResultStack.Pop().GetInteger();
            }
            return totalSupply;

        }

        public static BigInteger GetNep11BalanceOf(NeoSystem system, DataCache snapshot, UInt160 asset, string TokenId, UInt160 addr)
        {
            byte[] script;
            BigInteger decimals = 0;
            BigInteger balanceOf = 0;
            //先获取decimals来确定是不是可以分割的nft，如果是可以分割的nft，那么balanceOf（usr，tokenid），反为balanceOf（usr）
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitDynamicCall(asset, "decimals");
                script = sb.ToArray();
            }

            using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
            {
                if (engine.State.HasFlag(VMState.HALT))
                {
                    decimals = engine.ResultStack.Pop().GetInteger();
                }
            }
            //如果精度是0，那么tokenid一定只有一个地址拥有。查询ownerOf看是不是属于addr，是就返回1，不是就返回0
            if (decimals == 0)
            {
                //如果是有精度的就查询balanceof
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    try
                    {
                        try
                        {
                            sb.EmitDynamicCall(asset, "ownerOf", Convert.FromBase64String(TokenId));

                        }
                        catch(Exception e)
                        {
                            sb.EmitDynamicCall(asset, "ownerOf", Convert.FromHexString(TokenId));
                        }
                    }
                    catch(Exception e)
                    {
                        
                        sb.EmitDynamicCall(asset, "ownerOf", TokenId);
                    }
                    script = sb.ToArray();
                }
                using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
                {
                    if (engine.State.HasFlag(VMState.HALT))
                    {
                        var owner = new UInt160(engine.ResultStack.Pop().GetSpan().ToArray());
                        balanceOf = owner == addr ? 1 : 0;
                    }
                }
            }
            else
            {
                //如果是有精度的就查询balanceof
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    try
                    {
                        try
                        {
                            sb.EmitDynamicCall(asset, "balanceOf", addr == null ? UInt160.Parse("0x1100000000000000000220000000000000000011") : addr, Convert.FromBase64String(TokenId));
                        }
                        catch (Exception e)
                        {
                            sb.EmitDynamicCall(asset, "balanceOf", addr == null ? UInt160.Parse("0x1100000000000000000220000000000000000011") : addr, Convert.FromHexString(TokenId));
                        }
                    }
                    catch (Exception e)
                    {
                        sb.EmitDynamicCall(asset, "balanceOf", addr == null ? UInt160.Parse("0x1100000000000000000220000000000000000011") : addr, TokenId);
                    }
                    script = sb.ToArray();
                }
                using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
                {
                    if (engine.State.HasFlag(VMState.HALT))
                    {
                        balanceOf = engine.ResultStack.Pop().GetInteger();
                    }
                }
            }
            return balanceOf;
        }


        public static BigInteger[] GetNep17BalanceOf(NeoSystem system, DataCache snapshot, UInt160 asset,params UInt160[] addrs)
        {
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                foreach (var addr in addrs)
                {
                    sb.EmitDynamicCall(asset, "balanceOf", addr is null ? UInt160.Parse("0x1100000000000000000220000000000000000011") : addr);
                }
                script = sb.ToArray();
            }

            BigInteger[] balanceOfs = new BigInteger[addrs.Length];
            using (ApplicationEngine engine = ApplicationEngine.Run(script, snapshot, settings: system.Settings))
            {
                if (engine.State.HasFlag(VMState.HALT))
                {
                    for (var i = balanceOfs.Length - 1; i >= 0; i--)
                    {
                        balanceOfs[i] = engine.ResultStack.Pop().GetInteger();
                    }
                }
            }
            return balanceOfs;
        }
    }
}

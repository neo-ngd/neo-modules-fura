using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using System.Linq;
using Neo.VM;
using Neo.Extensions;
using Neo.Plugins;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.Plugins.Models
{
    [Collection("Execution")]
    public class ExecutionModel : Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [BsonElement("trigger")]
        public string Trigger { get; set; }

        [BsonElement("vmstate")]
        public string VmState { get; set; }

        [BsonElement("exception")]
        public string Exception { get; set; }

        [BsonElement("gasconsumed")]
        public long GasConsumed { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        [BsonElement("stacks")]
        public string[] Stacks { get; set; }

        public ExecutionModel() { }

        public ExecutionModel(UInt256 txid, UInt256 blockHash, ulong timestamp, string trigger, string vmstate, string exception, long gasconsumed, Neo.VM.Types.StackItem[] stack)
        {
            Txid = txid;
            BlockHash = blockHash;
            Trigger = trigger;
            VmState = vmstate;
            Exception = exception;
            GasConsumed = gasconsumed;
            Timestamp = timestamp;
            if(stack.Length < 500)
            {
                Stacks = stack.Select(p =>
                {
                    try
                    {
                        if(IsComplexObject(p))
                        {
                            Loger.Warning("IsComplexObject");
                            return "";
                        }
                        return p.ToJson().ToString();
                    }
                    catch
                    {
                        return "";
                    }
                }).ToArray();
            }
            else
            {
                Stacks = new string[] { };
            }
        }
        public static ExecutionModel Get(UInt256 txid,UInt256 blockHash, string trigger)
        {
            ExecutionModel executionModel = DB.Find<ExecutionModel>().Match( e => e.Txid == txid && e.BlockHash == blockHash && e.Trigger == trigger ).ExecuteFirstAsync().Result;
            return executionModel;
        }

        /// <summary>
        /// 查询某个高度是否已经存入execution
        /// </summary>
        /// <param name="blockHash"></param>
        /// <returns>true代表存在，false代表不存在</returns>
        public static bool ExistByBlockHash(UInt256 blockHash)
        {
            ExecutionModel executionModel = DB.Find<ExecutionModel>().Match( e => e.BlockHash == blockHash).ExecuteFirstAsync().Result;
            return executionModel is not null;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<ExecutionModel>( o => { o = new CreateCollectionOptions<ExecutionModel>(); });
            await DB.Index<ExecutionModel>().Key(a => a.Txid, KeyType.Ascending).Option(o => { o.Name = "_txid_"; }).CreateAsync();
            await DB.Index<ExecutionModel>().Key(a => a.BlockHash, KeyType.Ascending).Option(o => { o.Name = "_blockhash_"; }).CreateAsync();
            await DB.Index<ExecutionModel>().Key(a => a.Txid, KeyType.Ascending).Key(a => a.BlockHash, KeyType.Ascending).Key(a => a.Trigger, KeyType.Ascending).Option(o => { o.Name = "_txid_blockhash_trigger_"; }).CreateAsync();
        }

        /// <summary>
        /// 检查 StackItem 的复杂度，防止过于复杂的对象导致内存问题
        /// </summary>
        /// <param name="item">要检查的对象</param>
        /// <param name="maxDepth">最大允许深度</param>
        /// <param name="maxCount">最大允许元素总数</param>
        /// <param name="maxStringSize">最大允许字符串大小(字节)</param>
        /// <returns>如果对象过于复杂返回true</returns>
        public static bool IsComplexObject(Neo.VM.Types.StackItem item, int maxDepth = 20, int maxCount = 2000, int maxStringSize = 20000)
        {
            var visited = new HashSet<Neo.VM.Types.StackItem>();
            int totalCount = 0;
            return CheckComplexity(item, 0, visited, ref totalCount, maxDepth, maxCount, maxStringSize);
        }

        private static bool CheckComplexity(
            Neo.VM.Types.StackItem item, 
            int currentDepth,
            HashSet<Neo.VM.Types.StackItem> visited,
            ref int totalCount,
            int maxDepth, 
            int maxCount,
            int maxStringSize)
        {
            // 检查深度限制
            if (currentDepth > maxDepth)
                return true;
        
            // 检查元素总数限制
            if (totalCount > maxCount)
                return true;
        
            // 防止循环引用导致无限递归
            if (!visited.Add(item))
                return false; // 已访问过，不重复计算复杂度
        
            totalCount++;
        
            try
            {
                // 根据不同类型检查复杂度
                switch (item)
                {
                    case Neo.VM.Types.Array array:
                        // 检查数组元素数量
                        if (array.Count > 100) // 单个集合的元素数限制
                            return true;
                    
                        // 递归检查每个元素
                        foreach (var element in array)
                        {
                            if (CheckComplexity(element, currentDepth + 1, visited, ref totalCount, maxDepth, maxCount, maxStringSize))
                                return true;
                        }
                        break;
                    
                    case Neo.VM.Types.Map map:
                        // // 检查映射元素数量
                        // if (map.Count > 100) // 单个映射的键值对数限制
                        //     return true;
                    
                        // // 递归检查每个键和值
                        // foreach (var pair in map)
                        // {
                        //     if (CheckComplexity(pair.Key, currentDepth + 1, visited, ref totalCount, maxDepth, maxCount, maxStringSize))
                        //         return true;
                        
                        //     if (CheckComplexity(pair.Value, currentDepth + 1, visited, ref totalCount, maxDepth, maxCount, maxStringSize))
                        //         return true;
                        // }
                        break;
                    
                    case Neo.VM.Types.ByteString byteString:
                        // 检查字符串大小
                        // var size = byteString.GetSpan().Length;
                        // if (size > maxStringSize)
                        //     return true;
                        break;
                    
                    case Neo.VM.Types.Buffer buffer:
                        // // 检查缓冲区大小
                        // if (buffer.Size > maxStringSize)
                        //     return true;
                        break;
                    // 其他简单类型不需要特殊处理
                    case Neo.VM.Types.Boolean _:
                    case Neo.VM.Types.Integer _:
                    case Neo.VM.Types.Null _:
                    case Neo.VM.Types.InteropInterface _:
                    case Neo.VM.Types.Pointer _:
                        break;
                    default:
                        break;
                }
            
                return false; // 通过所有检查，不是复杂对象
            }
            catch
            {
                // 如果检查过程中出现异常，视为复杂对象
                return true;
            }
        }
    }
}

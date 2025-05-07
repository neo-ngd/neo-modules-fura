using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using System.Linq;
using Neo.VM;
using Neo.Extensions;

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
    }
}

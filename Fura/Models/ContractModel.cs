using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.IO.Json;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Contract")]
    public class ContractModel : Entity
    {
        [UInt160AsString]
        [BsonElement("hash")]
        public UInt160 Hash { get; set; }

        [BsonElement("id")]
        public int _ID { get; set; }

        [BsonElement("updatecounter")]
        public short UpdateCounter { get; set; }

        [BsonElement("nef")]
        public BsonString Nef { get; set; }

        [BsonElement("manifest")]
        public BsonString Manifest { get; set; }

        [BsonElement("createtime")]
        public ulong CreateTime { get; set; }

        [UInt256AsString]
        [BsonElement("createTxid")]
        public UInt256 CreateTxid { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        public ContractModel() { }

        public ContractModel(UInt160 hash,string name, int id, short updateCounter, JObject nef,JObject manifest, ulong createTime, UInt256 txid)
        {
            Hash = hash;
            Name = name;
            _ID = id;
            UpdateCounter = updateCounter;
            Nef = BsonString.Create(nef.ToString());
            Manifest = BsonString.Create(manifest.ToString());
            CreateTime = createTime;
            CreateTxid = txid;
        }

        public static ContractModel Get(UInt160 hash)
        {
            ContractModel contractModel = DB.Find<ContractModel>().Match(c => c.Hash == hash).ExecuteFirstAsync().Result;
            return contractModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<ContractModel>(new CreateCollectionOptions<ContractModel>());
            await DB.Index<ContractModel>().Key(a => a.Hash, KeyType.Ascending).Option(o => { o.Name = "_hash_"; }).CreateAsync();
            await DB.Index<ContractModel>().Key(a => a.Hash, KeyType.Ascending).Key(a => a.UpdateCounter, KeyType.Ascending).Option(o => { o.Name = "_hash_updatecounter_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

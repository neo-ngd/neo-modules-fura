using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("VerifyContractModel")]
    public class VerifyContractModel : Entity
    {
        [UInt160AsString]
        [BsonElement("hash")]
        public UInt160 Hash { get; set; }

        [BsonElement("id")]
        public int _ID { get; set; }

        [BsonElement("updatecounter")]
        public ushort UpdateCounter { get; set; }

        public VerifyContractModel(UInt160 hash, int _id, ushort updateCounter)
        {
            Hash = hash;
            _ID = _id;
            UpdateCounter = updateCounter;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<VerifyContractModel>(new CreateCollectionOptions<VerifyContractModel>());
        }
    }
}

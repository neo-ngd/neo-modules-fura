using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("GasBurn")]
    public class GasMintBurnModel : Entity
    {
        [BsonElement("burnAmount")]
        public BsonDecimal128 BurnAmount { get; set; }

        [BsonElement("mintAmount")]
        public BsonDecimal128 MintAmount { get; set; }

        [BsonElement("blockIndex")]
        public uint BlockIndex { get; set; }

        public GasMintBurnModel()
        {
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<GasMintBurnModel>(new CreateCollectionOptions<GasMintBurnModel>());
            await DB.Index<GasMintBurnModel>().Key(a => a.BlockIndex, KeyType.Ascending).Option(o => { o.Name = "_blockIndex_"; }).CreateAsync();
        }
    }
}

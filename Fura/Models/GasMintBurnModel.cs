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

        [BsonElement("totalBurnAmount")]
        public BsonDecimal128 TotalBurnAmount { get; set; }

        [BsonElement("mintAmount")]
        public BsonDecimal128 MintAmount { get; set; }

        [BsonElement("totalMintAmount")]
        public BsonDecimal128 TotalMintAmount { get; set; }

        [BsonElement("blockIndex")]
        public uint BlockIndex { get; set; }

        public GasMintBurnModel()
        {
        }

        public static GasMintBurnModel Get(uint BlockIndex)
        {
            GasMintBurnModel gasMintBurnModel = DB.Find<GasMintBurnModel>().Match(g => g.BlockIndex == BlockIndex).ExecuteFirstAsync().Result;
            return gasMintBurnModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<GasMintBurnModel>( o => { o = new CreateCollectionOptions<GasMintBurnModel>(); });
            await DB.Index<GasMintBurnModel>().Key(a => a.BlockIndex, KeyType.Ascending).Option(o => { o.Name = "_blockIndex_"; }).CreateAsync();
        }
    }
}

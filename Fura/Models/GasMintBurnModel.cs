using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
    }
}

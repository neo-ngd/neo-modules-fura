using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Block")]
    public class BlockModel : Entity
    {
        [BsonIgnore]
        public HeaderModel header { get; set; }

        [BsonElement("size")]
        public int Size { get; set; }

        [BsonElement("hash")]
        [UInt256AsString]
        public UInt256 Hash { get { return header.Hash; }}

        [BsonElement("version")]
        public uint Version { get { return header.Version; } }

        [UInt256AsString]
        [BsonElement("prevhash")]
        public UInt256 PrevHash { get { return header.PrevHash; } }

        [UInt256AsString]
        [BsonElement("merkleroot")]
        public UInt256 MerkleRoot { get { return header.MerkleRoot; } }

        [BsonElement("timestamp")]
        public ulong Timestamp { get { return header.Timestamp; } }

        [BsonElement("index")]
        public uint Index { get { return header.Index; } }

        [BsonElement("primary")]
        public byte PrimaryIndex { get { return header.PrimaryIndex; } }

        [UInt160AsString]
        [BsonElement("nextConsensus")]
        public UInt160 NextConsensus { get { return header.NextConsensus; } }

        [BsonElement("witnesses")]
        public WitnessModel[] Witnesses { get { return header.Witnesses; } }

        [BsonElement("systemFee")]
        public long SystemFee { get; set; }

        [BsonElement("networkFee")]
        public long NetworkFee { get; set; }

        [BsonElement("nonce")]
        public string Nonce { get; set; }

        public BlockModel(Block block)
        {
            header = new HeaderModel(block.Header);
            Size = block.Size;
            Nonce = block.Nonce.ToString();
        }

        public static BlockModel Get( UInt256 blockHash )
        {
            BlockModel blockModel = DB.Find<BlockModel>().Match( b => b.Hash == blockHash ).ExecuteFirstAsync().Result;
            return blockModel;
        }
    }
}

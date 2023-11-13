using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Header")]
    public class HeaderModel : Entity
    {
        [BsonElement("version")]
        public uint Version { get; set; }

        [UInt256AsString]
        [BsonElement("prevhash")]
        public UInt256 PrevHash { get; set; }

        [UInt256AsString]
        [BsonElement("merkleroot")]
        public UInt256 MerkleRoot { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        [BsonElement("index")]
        public uint Index { get; set; }

        [BsonElement("primaryindex")]
        public byte PrimaryIndex;

        [UInt160AsString]
        [BsonElement("nextConsensus")]
        public UInt160 NextConsensus { get; set; }

        [BsonElement("witnesses")]
        public WitnessModel[] Witnesses { get; set; }

        [UInt256AsString]
        [BsonElement("hash")]
        public UInt256 Hash { get; set; }

        [BsonElement("size")]
        public int Size { get; set; }

        public HeaderModel() { }

        public HeaderModel(Header header)
        {
            Version = header.Version;
            PrevHash = header.PrevHash;
            MerkleRoot = header.MerkleRoot;
            Timestamp = header.Timestamp;
            Index = header.Index;
            PrimaryIndex = header.PrimaryIndex;
            NextConsensus = header.NextConsensus;
            Witnesses = new WitnessModel[] { new(header.Witness)};
            Hash = header.Hash;
            Size = header.Size;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<HeaderModel>(new CreateCollectionOptions<HeaderModel>());
            await DB.Index<HeaderModel>().Key(a => a.Index, KeyType.Ascending).Option(o => { o.Name = "_index_unique_"; o.Unique = true; }).CreateAsync();
            await DB.Index<HeaderModel>().Key(a => a.Hash, KeyType.Ascending).Option(o => { o.Name = "_hash_unique_"; o.Unique = true; }).CreateAsync();
        }
    }

    public class WitnessModel
    {
        [BsonElement("invocation")]
        public string Invocation_B64String;

        [BsonElement("verification")]
        public string Verification_B64String;

        public WitnessModel(Witness witness)
        {
            Invocation_B64String = Convert.ToBase64String(witness.InvocationScript.ToArray());
            Verification_B64String = Convert.ToBase64String(witness.VerificationScript.ToArray());
        }

        public static WitnessModel[] ToModels(Witness[] witnesses)
        {
            var tms = new WitnessModel[witnesses.Length];
            for (var i = 0; i < witnesses.Length; i++)
            {
                tms[i] = new(witnesses[i]);
            }
            return tms;
        }
    }
}

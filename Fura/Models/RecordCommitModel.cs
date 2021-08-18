using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("RecordCommit")]
    public class RecordCommitModel : Entity
    {
        [BsonElement("blockIndex")]
        public uint BlockIndex { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("pName")]
        public string PName { get; set; }

        [BsonElement("timestamp")]
        public long Timestamp { get; set; }

        public static RecordCommitModel Get(uint blockIndex)
        {
            RecordCommitModel recordPersistModel = DB.Find<RecordCommitModel>().Match(r => r.BlockIndex == blockIndex).ExecuteFirstAsync().Result;
            return recordPersistModel;
        }

        public static void Delete(uint blockIndex)
        {
            DB.DeleteAsync<RecordCommitModel>(r => r.BlockIndex == blockIndex);
        }
    }
}

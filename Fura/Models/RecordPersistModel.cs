using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    public enum EnumRecordState
    {
        None,
        Pending,
        Confirm
    }

    [Collection("RecordPersist")]
    public class RecordPersistModel : Entity
    {
        [BsonElement("blockIndex")]
        public uint BlockIndex { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("pName")]
        public string PName { get; set; }

        [BsonElement("timestamp")]
        public long Timestamp { get; set; }

        public static RecordPersistModel Get(uint blockIndex)
        {
            RecordPersistModel recordPersistModel = DB.Find<RecordPersistModel>().Match(r => r.BlockIndex == blockIndex).ExecuteFirstAsync().Result;
            return recordPersistModel;
        }

        public static void Delete(uint blockIndex)
        {
            DB.DeleteAsync<RecordPersistModel>(r => r.BlockIndex == blockIndex);
        }
    }
}

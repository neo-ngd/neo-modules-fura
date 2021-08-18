using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("Address")]
    public class AddressModel : Entity
    {
        [UInt160AsString]
        [BsonElement("address")]
        public UInt160 Address { get; set; }

        [BsonElement("firstusetime")]
        public ulong FirstUseTime { get; set; }//timestamp

        public AddressModel(UInt160 address, ulong firstUseTime)
        {
            Address = address;
            FirstUseTime = firstUseTime;
        }

        public AddressModel(AddressModel addressModel)
        {
            ID = addressModel.ID;
            Address = addressModel.Address;
            FirstUseTime = addressModel.FirstUseTime;
        }

        public static AddressModel Get(UInt160 address)
        {
            AddressModel addressModel = DB.Find<AddressModel>().Match(a => a.Address == address).ExecuteFirstAsync().Result;
            return addressModel;
        }
    }
}

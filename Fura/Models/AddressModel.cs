using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
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

        public AddressModel()
        {

        }

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

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<AddressModel>( o => { o = new CreateCollectionOptions<AddressModel>(); });
            await DB.Index<AddressModel>().Key(a => a.Address, KeyType.Ascending).Option(o => { o.Name = "_address_unique_"; o.Unique = true; }).CreateAsync();
            await DB.Index<AddressModel>().Key(a => a.FirstUseTime, KeyType.Ascending).Option(o => { o.Name = "_firstusetime_"; }).CreateAsync();
        }
    }
}

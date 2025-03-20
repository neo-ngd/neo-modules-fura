using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    public enum EnumAssetType
    {
        Unknown,
        NEP11,
        NEP17
    }


    [Collection("Asset")]
    public class AssetModel : Entity
    {
        [UInt160AsString]
        [BsonElement("hash")]
        public UInt160 Hash { get; set; }

        [BsonElement("firsttransfertime")]
        public ulong FirstTransferTime { get; set; }

        [BsonElement("tokenname")]
        public string TokenName { get; set; }

        [BsonElement("decimals")]
        public byte Decimals { get; set; }

        [BsonElement("symbol")]
        public string Symbol { get; set; }

        [BsonElement("totalsupply")]
        public BsonDecimal128 TotalSupply { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        public AssetModel()
        {

        }

        public AssetModel(UInt160 hash,ulong firstTransferTime, string tokenName, byte decimals, string symbol, BigInteger totalSupply, EnumAssetType enumAssetType)
        {
            Hash = hash;
            FirstTransferTime = firstTransferTime;
            TokenName = tokenName;
            Decimals = decimals;
            Symbol = symbol;
            TotalSupply = BsonDecimal128.Create(totalSupply.ToString().WipeNumStrToFitDecimal128());
            Type = enumAssetType.ToString();
        }

        public static AssetModel Get(UInt160 hash)
        {
            AssetModel assetModel = DB.Find<AssetModel>().Match( a => a.Hash == hash).ExecuteFirstAsync().Result;
            return assetModel;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<AssetModel>( o => { o = new CreateCollectionOptions<AssetModel>(); });
            await DB.Index<AssetModel>().Key(a => a.Hash, KeyType.Ascending).Option(o => { o.Name = "_hash_unique_"; o.Unique = true; }).CreateAsync();
        }
    }
}

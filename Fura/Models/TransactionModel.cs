using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Network.P2P.Payloads;
using Neo.Plugins.Attribute;
using Neo.Wallets;

namespace Neo.Plugins.Models
{
    [Collection("Transaction")]
    public class TransactionModel : Entity
    {
        [UInt256AsString]
        [BsonElement("hash")]
        public UInt256 Hash { get; set; }

        [BsonElement("size")]
        public int Size { get; set; }

        [BsonElement("version")]
        public byte Version { get; set; }

        [BsonElement("nonce")]
        public long Nonce { get; set; }

        [BsonElement("sender")]
        public string Sender { get; set; }

        [BsonElement("sysfee")]
        public long Sysfee { get; set; }

        [BsonElement("netfee")]
        public long Netfee { get; set; }

        [BsonElement("validUntilBlock")]
        public uint ValidUntilBlock { get; set; }

        [BsonElement("signers")]
        public SignerModel[] Signers { get; set; }

        [BsonElement("attributes")]
        public TransactionAttributeModel[] Attributes { get; set; }

        [ByteArrayAsBase64]
        [BsonElement("script")]
        public byte[] Script { get; set; }

        [BsonElement("witnesses")]
        public WitnessModel[] Witnesses { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [BsonElement("blockIndex")]
        public uint BlockIndex { get; set; }

        [BsonElement("blocktime")]
        public ulong BlockTime { get; set; }

        public TransactionModel() { }

        public TransactionModel(Neo.Network.P2P.Payloads.Transaction transaction,UInt256 blockHash, ulong blockTime, uint blockIndex)
        {
            Hash = transaction.Hash;
            Size = transaction.Size;
            Version =  transaction.Version;
            Nonce = transaction.Nonce;
            Sender = transaction.Sender.ToAddress(0x35);
            Sysfee = transaction.SystemFee;
            Netfee = transaction.NetworkFee;
            ValidUntilBlock = transaction.ValidUntilBlock;
            Signers = SignerModel.ToModels(transaction.Signers);
            Attributes = TransactionAttributeModel.ToModels(transaction.Attributes);
            Script = transaction.Script;
            Witnesses = WitnessModel.ToModels(transaction.Witnesses);
            BlockHash = blockHash;
            BlockTime = blockTime;
            BlockIndex = blockIndex;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<TransactionModel>(new CreateCollectionOptions<TransactionModel>());
            await DB.Index<TransactionModel>().Key(a => a.Hash, KeyType.Ascending).Option(o => { o.Name = "_hash_unique_"; o.Unique = true; }).CreateAsync();
            await DB.Index<TransactionModel>().Key(a => a.Sender, KeyType.Ascending).Option(o => { o.Name = "_sender_"; }).CreateAsync();
            await DB.Index<TransactionModel>().Key(a => a.BlockHash, KeyType.Ascending).Option(o => { o.Name = "_blockhash_"; }).CreateAsync();
            await DB.Index<TransactionModel>().Key(a => a.BlockTime, KeyType.Ascending).Option(o => { o.Name = "_blocktime_"; }).CreateAsync();
            await DB.Index<TransactionModel>().Key(a => a.BlockIndex, KeyType.Ascending).Option(o => { o.Name = "_blockIndex_"; }).CreateAsync();
        }
    }


    public class SignerModel
    {
        [UInt160AsString]
        [BsonElement("account")]
        public UInt160 Account;

        [BsonElement("scopes")]
        public string Scopes;

        public SignerModel(Signer signer)
        {
            Account = signer.Account;
            Scopes = signer.Scopes.ToString();
        }

        public static SignerModel[] ToModels(Signer[] signers)
        {
            SignerModel[] models = signers.Select(signer => new SignerModel(signer)).ToArray();
            return models;
        }
    }


    public class TransactionAttributeModel
    {
        [BsonElement("type")]
        public string Type;

        public TransactionAttributeModel(TransactionAttribute attribute)
        {
            Type = attribute.Type.ToString();
        }

        public static TransactionAttributeModel[] ToModels(TransactionAttribute[] transactionAttributes)
        {
            TransactionAttributeModel[] models = transactionAttributes.Select(ta => new TransactionAttributeModel(ta)).ToArray();
            return models;
        }
    }
}

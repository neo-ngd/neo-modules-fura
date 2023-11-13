using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;

namespace Neo.Plugins
{
    public class MongoClient
    {
        public static async Task InitDB(string dbName, string host, int port, string user, string password)
        {
            await DB.InitAsync(dbName, new MongoClientSettings()
            {
                MaxConnectionPoolSize = 200,
                Server = new MongoServerAddress(host, port),
                Credential = MongoCredential.CreateCredential(dbName, user, password)
            });
            Loger.Common("mongodb init succ");
        }

        public static async Task InitDB(string dbName, string url)
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(url));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.MaxConnectionPoolSize = 200;
            await DB.InitAsync(dbName, settings);
            Loger.Common("mongodb init succ");
        }

        public static async Task InitCollectionAndIndex()
        {
            var assembly = Assembly.Load(File.ReadAllBytes("Plugins/Fura/Fura.dll"));

            foreach (Type type in assembly.ExportedTypes)
            {
                if (!type.IsSubclassOf(typeof(Entity))) continue;
                if (type.IsAbstract) continue;
                MethodInfo methodInfo = type.GetMethod("InitCollectionAndIndex", BindingFlags.Static | BindingFlags.Public);
                try
                {
                    var t = (Task)methodInfo?.Invoke(null, null);
                    await t;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Loger.Common("collection and index init succ");
        }

        public static async Task InitBasicData(NeoSystem system)
        {
            AssetModel assetModel = (from a in DB.Queryable<AssetModel>() where a.Hash.Equals(NativeContract.GAS.Hash) select a).FirstOrDefault();
            if (assetModel is not null)
                return;
            using (var transaction = new MongoDB.Entities.Transaction())
            {
                ContractModel contractModel_oracle = new(NativeContract.Oracle.Hash, "Oracle", NativeContract.Oracle.Id, 0, NativeContract.Oracle.Nef.ToJson(), NativeContract.Oracle.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_roleManagement = new(NativeContract.RoleManagement.Hash, "RoleManagement", NativeContract.RoleManagement.Id, 0, NativeContract.RoleManagement.Nef.ToJson(), NativeContract.RoleManagement.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_policy = new(NativeContract.Policy.Hash, "Policy", NativeContract.Policy.Id, 0, NativeContract.Policy.Nef.ToJson(), NativeContract.Policy.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_gas = new(NativeContract.GAS.Hash, "GasToken", NativeContract.GAS.Id, 0, NativeContract.GAS.Nef.ToJson(), NativeContract.GAS.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_neo = new(NativeContract.NEO.Hash, "NeoToken", NativeContract.NEO.Id, 0, NativeContract.NEO.Nef.ToJson(), NativeContract.NEO.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_ledger = new(NativeContract.Ledger.Hash, "Ledger", NativeContract.Ledger.Id, 0, NativeContract.Ledger.Nef.ToJson(), NativeContract.Ledger.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_cryptoLib = new(NativeContract.CryptoLib.Hash, "CryptoLib", NativeContract.CryptoLib.Id, 0, NativeContract.CryptoLib.Nef.ToJson(), NativeContract.CryptoLib.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_stdLib = new(NativeContract.StdLib.Hash, "StdLib", NativeContract.StdLib.Id, 0, NativeContract.StdLib.Nef.ToJson(), NativeContract.StdLib.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                ContractModel contractModel_contractManagement = new(NativeContract.ContractManagement.Hash, "ContractManagement", NativeContract.ContractManagement.Id, 0, NativeContract.ContractManagement.Nef.ToJson(), NativeContract.ContractManagement.Manifest.ToJson(), system.GenesisBlock.Timestamp, UInt256.Zero);
                await transaction.SaveAsync(contractModel_oracle);
                await transaction.SaveAsync(contractModel_roleManagement);
                await transaction.SaveAsync(contractModel_policy);
                await transaction.SaveAsync(contractModel_gas);
                await transaction.SaveAsync(contractModel_neo);
                await transaction.SaveAsync(contractModel_ledger);
                await transaction.SaveAsync(contractModel_cryptoLib);
                await transaction.SaveAsync(contractModel_stdLib);
                await transaction.SaveAsync(contractModel_contractManagement);
                AssetModel assetModel_gas = new(NativeContract.GAS.Hash, system.GenesisBlock.Timestamp, "GasToken", 8, "GAS", 0, EnumAssetType.NEP17);
                AssetModel assetModel_neo = new(NativeContract.NEO.Hash, system.GenesisBlock.Timestamp, "NeoToken", 0, "NEO", 0, EnumAssetType.NEP17);
                await transaction.SaveAsync(assetModel_gas);
                await transaction.SaveAsync(assetModel_neo);
                VerifyContractModel verifyContractModel_oracle = new(NativeContract.Oracle.Hash, NativeContract.Oracle.Id, 0);
                VerifyContractModel verifyContractModel_roleManagement = new(NativeContract.RoleManagement.Hash, NativeContract.RoleManagement.Id, 0);
                VerifyContractModel verifyContractModel_policy = new(NativeContract.Policy.Hash, NativeContract.Policy.Id, 0);
                VerifyContractModel verifyContractModel_gas = new(NativeContract.GAS.Hash, NativeContract.GAS.Id, 0);
                VerifyContractModel verifyContractModel_neo = new(NativeContract.NEO.Hash, NativeContract.NEO.Id, 0);
                VerifyContractModel verifyContractModel_ledger = new(NativeContract.Ledger.Hash, NativeContract.Ledger.Id, 0);
                VerifyContractModel verifyContractModel_cryptoLib = new(NativeContract.CryptoLib.Hash, NativeContract.CryptoLib.Id, 0);
                VerifyContractModel verifyContractModel_stdLib = new(NativeContract.StdLib.Hash, NativeContract.StdLib.Id, 0);
                VerifyContractModel verifyContractModel_contractManagement = new(NativeContract.ContractManagement.Hash, NativeContract.ContractManagement.Id, 0);
                await transaction.SaveAsync(verifyContractModel_oracle);
                await transaction.SaveAsync(verifyContractModel_roleManagement);
                await transaction.SaveAsync(verifyContractModel_policy);
                await transaction.SaveAsync(verifyContractModel_gas);
                await transaction.SaveAsync(verifyContractModel_neo);
                await transaction.SaveAsync(verifyContractModel_ledger);
                await transaction.SaveAsync(verifyContractModel_cryptoLib);
                await transaction.SaveAsync(verifyContractModel_stdLib);
                await transaction.SaveAsync(verifyContractModel_contractManagement);
                await transaction.CommitAsync();
            }
            Loger.Common("data init succ");
        }
    }
}

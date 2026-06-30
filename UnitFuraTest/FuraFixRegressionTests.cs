using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo;
using Neo.Extensions;
using Neo.Persistence;
using Neo.Persistence.Providers;
using Neo.Plugins;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using Neo.VM.Types;

namespace UnitFuraTest
{
    [TestClass]
    public class FuraFixRegressionTests
    {
        private static readonly UInt160 SampleAddress = UInt160.Parse("0x9ed3a29b7f1599582ee1a9933f88e3b5e5b5c5e5");

        [TestMethod]
        public void AllEntityTypes_ShouldDefineInitCollectionAndIndex()
        {
            var missing = typeof(Fura).Assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Entity)) && !t.IsAbstract)
                .Where(t => t.GetMethod("InitCollectionAndIndex", BindingFlags.Public | BindingFlags.Static) is null)
                .Select(t => t.Name)
                .ToArray();

            Assert.AreEqual(0, missing.Length, $"Missing InitCollectionAndIndex: {string.Join(", ", missing)}");
        }

        [TestMethod]
        public void CandidateState_ToStackItem_Neo310()
        {
            var state = FuraTestAccess.CreateCandidateState(true, BigInteger.One);
            var item = FuraTestAccess.CandidateStateToStackItem(state);
            Assert.IsInstanceOfType(item, typeof(Struct));

            var roundTrip = FuraTestAccess.CreateCandidateState(false, BigInteger.Zero);
            FuraTestAccess.CandidateStateFromStackItem(roundTrip, item);
            Assert.IsTrue(FuraTestAccess.GetCandidateRegistered(roundTrip));
            Assert.AreEqual(BigInteger.One, FuraTestAccess.GetCandidateVotes(roundTrip));
        }

        [TestMethod]
        public void TryParseBase64ToScriptHash_RejectsNullOrEmpty()
        {
            Assert.IsFalse(FuraTestAccess.TryParseBase64ToScriptHash(null, out _));
            Assert.IsFalse(FuraTestAccess.TryParseBase64ToScriptHash("", out _));
            Assert.IsFalse(FuraTestAccess.TryParseBase64ToScriptHash(
                Convert.ToBase64String(ReadOnlySpan<byte>.Empty.ToArray()), out _));
        }

        [TestMethod]
        public void TryParseBase64ToScriptHash_ParsesValidHash160()
        {
            var base64 = Convert.ToBase64String(SampleAddress.GetSpan().ToArray());

            Assert.IsTrue(FuraTestAccess.TryParseBase64ToScriptHash(base64, out var addr));
            Assert.AreEqual(SampleAddress, addr);
        }

        [TestMethod]
        public void IsNullStackItem_DetectsGasMintFromField()
        {
            Assert.IsTrue(FuraTestAccess.IsNullStackItem(null));
            Assert.IsTrue(FuraTestAccess.IsNullStackItem(new NotificationStateValueModel(StackItem.Null)));
            Assert.IsTrue(FuraTestAccess.IsNullStackItem(new NotificationStateValueModel(new ByteString(ReadOnlyMemory<byte>.Empty))));
            Assert.IsFalse(FuraTestAccess.IsNullStackItem(new NotificationStateValueModel(new ByteString(SampleAddress.GetSpan().ToArray().AsMemory()))));
        }

        [TestMethod]
        public void TryParseNotificationHash_SkipsNullMintFrom()
        {
            Assert.IsFalse(FuraTestAccess.TryParseNotificationHash(
                new NotificationStateValueModel(StackItem.Null), out _));

            Assert.IsTrue(FuraTestAccess.TryParseNotificationHash(
                new NotificationStateValueModel(new ByteString(SampleAddress.GetSpan().ToArray().AsMemory())), out var to));
            Assert.AreEqual(SampleAddress, to);
        }

        [TestMethod]
        public void GetPreviousTotals_ReturnsZeroWhenPreviousBlockMissing()
        {
            var (burn, mint) = FuraTestAccess.GetPreviousTotals(null);
            Assert.AreEqual(BigInteger.Zero, burn);
            Assert.AreEqual(BigInteger.Zero, mint);
        }

        [TestMethod]
        public void GetPreviousTotals_ReadsPreviousCumulativeTotals()
        {
            var previous = new GasMintBurnModel
            {
                TotalBurnAmount = BsonDecimal128.Create("100"),
                TotalMintAmount = BsonDecimal128.Create("200")
            };

            var (burn, mint) = FuraTestAccess.GetPreviousTotals(previous);
            Assert.AreEqual(new BigInteger(100), burn);
            Assert.AreEqual(new BigInteger(200), mint);
        }

        [TestMethod]
        public void GetContractModel_ReturnsNullWhenContractNotDeployed()
        {
            using var store = new MemoryStore();
            using var snapshot = new StoreCache(store);

            var treasury = MongoClient.GetContractModel(snapshot, NativeContract.Treasury, 0);
            Assert.IsNull(treasury);
        }

        [TestMethod]
        public void GetContractModel_ReturnsNeoOnGenesisSnapshot()
        {
            using var system = new NeoSystem(TestProtocolSettings.Default, new MemoryStoreProvider());
            using var snapshot = system.GetSnapshotCache();

            var neo = MongoClient.GetContractModel(snapshot, NativeContract.NEO, system.GenesisBlock.Timestamp);
            Assert.IsNotNull(neo);
            Assert.AreEqual(NativeContract.NEO.Hash, neo.Hash);
        }
    }
}

using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using System.Numerics;

namespace Neo.Plugins.Cache
{
    public class CacheGasMintBurnParams
    {
        public BigInteger BurnAmount;
        public BigInteger MintAmount;
        public uint BlockIndex;
    }
    
    public class CacheGasMintBurn : IDBCache
    {
        private CacheGasMintBurnParams GasMintBurnParams;

        public CacheGasMintBurn()
        {
            GasMintBurnParams = new CacheGasMintBurnParams();
        }

        public void Clear()
        {
            GasMintBurnParams = new CacheGasMintBurnParams();
        }

        public void Save(Transaction tran)
        {
            if(GasMintBurnParams.BlockIndex > 0 || GasMintBurnParams.BurnAmount > 0 || GasMintBurnParams.MintAmount > 0)
            {
                GasMintBurnModel gasMintBurnModel = new GasMintBurnModel() { BurnAmount = BsonDecimal128.Create(GasMintBurnParams.BurnAmount.ToString()), MintAmount = BsonDecimal128.Create(GasMintBurnParams.MintAmount.ToString()), BlockIndex = GasMintBurnParams.BlockIndex };
                tran.SaveAsync(gasMintBurnModel).Wait();
            }
        }

        public void Add(uint blockIndex, BigInteger burnAmount, BigInteger mintAmount)
        {
            lock (GasMintBurnParams)
            {
                GasMintBurnParams.BlockIndex = blockIndex;
                GasMintBurnParams.BurnAmount += burnAmount;
                GasMintBurnParams.MintAmount += mintAmount;
            }
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }
    }
}

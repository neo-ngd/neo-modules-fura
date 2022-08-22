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
            BigInteger totalBurnAmount = 0;
            BigInteger totalMintAmount = 0;
            if(GasMintBurnParams.BlockIndex > 0)
            {
                //获取上一个块的total来计算本块的数据
                GasMintBurnModel gasMintBurnModel_Pre = GasMintBurnModel.Get(GasMintBurnParams.BlockIndex - 1);
                totalBurnAmount = BigInteger.Parse(gasMintBurnModel_Pre.TotalBurnAmount.ToString());
                totalMintAmount = BigInteger.Parse(gasMintBurnModel_Pre.TotalMintAmount.ToString());
            }
            GasMintBurnModel gasMintBurnModel = new GasMintBurnModel()
            {
                BurnAmount = BsonDecimal128.Create(GasMintBurnParams.BurnAmount.ToString().WipeNumStrToFitDecimal128()),
                TotalBurnAmount = BsonDecimal128.Create((totalBurnAmount + GasMintBurnParams.BurnAmount).ToString().WipeNumStrToFitDecimal128()),
                MintAmount = BsonDecimal128.Create(GasMintBurnParams.MintAmount.ToString().WipeNumStrToFitDecimal128()),
                TotalMintAmount = BsonDecimal128.Create((totalMintAmount + GasMintBurnParams.MintAmount).ToString().WipeNumStrToFitDecimal128()),
                BlockIndex = GasMintBurnParams.BlockIndex
            };
            tran.SaveAsync(gasMintBurnModel).Wait();
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

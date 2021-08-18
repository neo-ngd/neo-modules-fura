using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheAddressParams
    {
        public UInt160 Address;
        public ulong CreateTime;
    }

    public class CacheAddress : IDBCache
    {
        private ConcurrentDictionary<UInt160, CacheAddressParams> D_Address;
        private ConcurrentDictionary<UInt160, AddressModel> D_AddressModel;

        public CacheAddress()
        {
            D_AddressModel = new ConcurrentDictionary<UInt160, AddressModel>();
            D_Address = new ConcurrentDictionary<UInt160, CacheAddressParams>();
        }

        public void Clear()
        {
            D_AddressModel = new ConcurrentDictionary<UInt160, AddressModel>();
            D_Address = new ConcurrentDictionary<UInt160, CacheAddressParams>();
        }

        public void AddNeedUpdate(UInt160 address, ulong createTime)
        {
            if (address == UInt160.Zero || address is null)
                return;
            D_Address[address] = new(){ Address = address, CreateTime = createTime };
        }

        public List<CacheAddressParams> GetNeedUpdate()
        {
            return D_Address.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheAddressParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                Add(list[i].CreateTime, list[i].Address);
            });
        }

        public AddressModel Get(UInt160 addr)
        {
            if (D_AddressModel.ContainsKey(addr))
            {
                return D_AddressModel[addr];
            }
            else
            {
                return AddressModel.Get(addr);
            }
        }

        public void Add(ulong createTime, params UInt160[] addrs)
        {
            foreach (var addr in addrs)
            {
                if (addr is null)
                    continue;
                AddressModel addressModel = Get(addr);
                if (addressModel is null)
                {
                    addressModel = new(addr, createTime);
                    D_AddressModel[addr] = addressModel;
                }
            }
        }

        public void Save(Transaction tran)
        {
            if (D_AddressModel.Values.Count > 0)
                tran.SaveAsync(D_AddressModel.Values).Wait();
        }
    }
}

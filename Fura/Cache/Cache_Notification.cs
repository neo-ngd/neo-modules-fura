using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;

namespace Neo.Plugins.Cache
{
    public class CacheNotification : IDBCache
    {
        private ConcurrentBag<NotificationModel> L_NotificationModel;

        public CacheNotification()
        {
            L_NotificationModel = new ConcurrentBag<NotificationModel>();
        }


        public void Clear()
        {
            L_NotificationModel = new ConcurrentBag<NotificationModel>();
        }

        public void Add(List<NotificationModel>  notificationModels)
        {
            foreach (var notificationModel in notificationModels)
            {
                L_NotificationModel.Add(notificationModel);
            }
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
        }

        public void Save(Transaction tran)
        {
            if (L_NotificationModel.Count > 0)
                tran.SaveAsync(L_NotificationModel).Wait();
        }
    }
}

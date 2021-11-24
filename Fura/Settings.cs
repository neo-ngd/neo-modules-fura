using System;
using Microsoft.Extensions.Configuration;

namespace Neo.Plugins
{
    internal class Settings
    {
        public string DbName { get; }
        public string Host { get; }
        public int Port { get; }
        public string User { get; }
        public string Password { get; }
        public bool Log { get; }
        public string ConnectionString { get; }

        public string PName { get; }

        public static Settings Default { get; private set; }

        public int SleepTime { get; set; }

        public int WaitTime { get; set; }

        public int MarketContractId { get; set; }

        private Settings(IConfigurationSection section)
        {
            this.DbName = section.GetValue("DbName", "neo");
            this.Host = section.GetValue("Host", "127.0.0.1");
            this.Port = section.GetValue("Port", 27017);
            this.User = section.GetValue("User", "admin");
            this.Password = section.GetValue("Password","admin");
            this.ConnectionString = section.GetValue("ConnectionString", "");
            this.Log = section.GetValue("Log", true);
            Console.WriteLine(Environment.CurrentDirectory);
            this.PName = section.GetValue("PName", Environment.CurrentDirectory);
            this.SleepTime = section.GetValue("SleepTime", 10);
            this.WaitTime = section.GetValue("WaitTime", 900);
            this.MarketContractId = section.GetValue("MarketContractId", 0);
        }

        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}

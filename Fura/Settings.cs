using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

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

        public int SleepTime { get; }

        public int WaitTime { get;}

        public IReadOnlyList<int> MarketContractIds { get; }

        public IReadOnlyList<int> Nep11ContractIds { get; }

        public IReadOnlyList<int> Nep17ContractIds { get; }

        public IReadOnlyList<string> IlexContractHashes { get; }

        public IReadOnlyList<string> MetaContractHashes { get; }

        public string NNS { get; }

        private Settings(IConfigurationSection section)
        {
            this.DbName = section.GetValue("DbName", "neo");
            this.Host = section.GetValue("Host", "127.0.0.1");
            this.Port = section.GetValue("Port", 27017);
            this.User = section.GetValue("User", "admin");
            this.Password = section.GetValue("Password", "admin");
            this.ConnectionString = section.GetValue("ConnectionString", "");
            this.Log = section.GetValue("Log", true);
            Console.WriteLine(Environment.CurrentDirectory);
            this.PName = section.GetValue("PName", Environment.CurrentDirectory);
            this.SleepTime = section.GetValue("SleepTime", 10);
            this.WaitTime = section.GetValue("WaitTime", 900);
            this.MarketContractIds = section.GetSection("MarketContractIds").Exists()
                ? section.GetSection("MarketContractIds").GetChildren().Select(p => int.Parse(p.Value)).ToArray()
                : new[] { 0 };
            this.Nep11ContractIds = section.GetSection("Nep11ContractIds").Exists()
                ? section.GetSection("Nep11ContractIds").GetChildren().Select(p => int.Parse(p.Value)).ToArray()
                : new[] { 0 };
            this.Nep17ContractIds = section.GetSection("Nep17ContractIds").Exists()
                ? section.GetSection("Nep17ContractIds").GetChildren().Select(p => int.Parse(p.Value)).ToArray()
                : new[] { 0 };
            this.IlexContractHashes = section.GetSection("IlexContractHashes").Exists()
                ? section.GetSection("IlexContractHashes").GetChildren().Select(p => p.Value).ToArray()
                : new string[] { };
            this.MetaContractHashes = section.GetSection("MetaContractHashes").Exists()
                ? section.GetSection("MetaContractHashes").GetChildren().Select(p => p.Value).ToArray()
                : new string[] { };
            this.NNS = section.GetValue("NNS", "");

        }

        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}

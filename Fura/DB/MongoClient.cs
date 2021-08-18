using System.Security.Authentication;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;

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
        }

        public static async Task InitDB(string dbName, string url)
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(url));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.MaxConnectionPoolSize = 200;
            await DB.InitAsync(dbName, settings);
        }
    }
}

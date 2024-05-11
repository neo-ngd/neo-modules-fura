using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;

namespace Neo.Plugins.Models
{
    [Collection("Debug")]
    public class DebugModel : Entity
    {
        public string Info { get; set; }

        public DebugModel() { }

        public DebugModel(string info)
        {
            Info = info;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollectionAsync<DebugModel>( o => { o = new CreateCollectionOptions<DebugModel>(); });
        }
    }
}

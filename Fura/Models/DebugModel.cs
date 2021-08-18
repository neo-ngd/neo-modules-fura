using MongoDB.Entities;

namespace Neo.Plugins.Models
{
    [Collection("Debug")]
    public class DebugModel : Entity
    {
        public string Info { get; set; }

        public DebugModel(string info)
        {
            Info = info;
        }
    }
}

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Neo.Plugins.Attribute
{
    public class UInt160AsStringAttribute: BsonSerializerAttribute
    {
        public UInt160AsStringAttribute() : base(typeof(UInt160Serializer)) { }

        private class UInt160Serializer : SerializerBase<UInt160>
        {
            public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, UInt160 value)
            {
                if (value == null)
                {
                    ctx.Writer.WriteNull();
                    return;
                }
                ctx.Writer.WriteString(value.ToString());
            }


            public override UInt160 Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
            {
                switch (ctx.Reader.CurrentBsonType)
                {
                    case BsonType.String:
                        {
                            UInt160 result;
                            string str = ctx.Reader.ReadString();
                            var suc = UInt160.TryParse(str, out result);
                            if (suc)
                            {
                                return result;
                            }
                            else
                            {
                                return UInt160.Zero;
                            }
                        }
                    case BsonType.Null:
                        ctx.Reader.ReadNull();
                        return UInt160.Zero;

                    default:
                        throw new BsonSerializationException($"'{ctx.Reader.CurrentBsonType}' values are not valid on properties decorated with an [AsObjectId] attribute!");
                }
            }
        }
    }
}

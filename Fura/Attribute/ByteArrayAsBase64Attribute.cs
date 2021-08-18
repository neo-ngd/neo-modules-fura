using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Neo.Plugins.Attribute
{
    public class ByteArrayAsBase64Attribute : BsonSerializerAttribute
    {
        public ByteArrayAsBase64Attribute() : base(typeof(ByteArraySerializer)) { }

        private class ByteArraySerializer : SerializerBase<byte[]>
        {
            public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, byte[] value)
            {
                if (value == null)
                {
                    ctx.Writer.WriteNull();
                    return;
                }
                ctx.Writer.WriteString(Convert.ToBase64String(value));
            }


            public override byte[] Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
            {
                switch (ctx.Reader.CurrentBsonType)
                {
                    case BsonType.String:
                        {
                            return Convert.FromBase64String(ctx.Reader.ReadString());
                        }
                    case BsonType.Null:
                        ctx.Reader.ReadNull();
                        return null;
                    default:
                        throw new BsonSerializationException($"'{ctx.Reader.CurrentBsonType}' values are not valid on properties decorated with an [AsObjectId] attribute!");
                }
            }
        }
    }
}

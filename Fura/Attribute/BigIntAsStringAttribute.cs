using System.Numerics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Neo.Plugins.Attribute
{
    public class BigIntAsStringAttribute : BsonSerializerAttribute
    {
        public BigIntAsStringAttribute() : base(typeof(BigIntSerializer)) { }

        private class BigIntSerializer : SerializerBase<BigInteger>
        {
            public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, BigInteger value)
            {
                ctx.Writer.WriteString(value.ToString());
            }


            public override BigInteger Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
            {
                switch (ctx.Reader.CurrentBsonType)
                {
                    case BsonType.String:
                        {
                            return BigInteger.Parse(ctx.Reader.ReadString());
                        }
                    case BsonType.Null:
                        ctx.Reader.ReadNull();
                        return BigInteger.Zero;
                    default:
                        throw new BsonSerializationException($"'{ctx.Reader.CurrentBsonType}' values are not valid on properties decorated with an [AsObjectId] attribute!");
                }
            }
        }
    }
}

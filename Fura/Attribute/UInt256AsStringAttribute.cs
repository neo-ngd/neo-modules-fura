using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Neo.Plugins.Attribute
{
    public class UInt256AsStringAttribute: BsonSerializerAttribute
    {
        public UInt256AsStringAttribute() : base(typeof(UInt256Serializer)) { }

        private class UInt256Serializer : SerializerBase<UInt256>
        {
            public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, UInt256 value)
            {
                if (value == null)
                {
                    ctx.Writer.WriteNull();
                    return;
                }
                ctx.Writer.WriteString(value.ToString());
            }


            public override UInt256 Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
            {
                switch (ctx.Reader.CurrentBsonType)
                {
                    case BsonType.String:
                        {
                            UInt256 result;
                            string str = ctx.Reader.ReadString();
                            var suc =  UInt256.TryParse(str,out result);
                            if (suc)
                            {
                                return result;
                            }
                            else
                            {
                                return UInt256.Zero;
                            }
                        }
                    case BsonType.Null:
                        ctx.Reader.ReadNull();
                        return UInt256.Zero;

                    default:
                        throw new BsonSerializationException($"'{ctx.Reader.CurrentBsonType}' values are not valid on properties decorated with an [AsObjectId] attribute!");
                }
            }
        }
    }
}

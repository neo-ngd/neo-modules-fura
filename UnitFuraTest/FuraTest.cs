using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Neo;
using System.Linq;
using Neo.SmartContract;
using System.Numerics;
using System.Text;
using Neo.Plugins.VM;
using Neo.Plugins;
using Neo.Cryptography.ECC;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Neo.VM;
using Neo.Cryptography;

namespace UnitFuraTest
{
    [TestClass]
    public class FuraTest
    {
        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void TestScript2Executions()
        {
            var aa = new UInt256((UTF8Encoding.UTF8.GetBytes("Auction")).Sha256());
            var ba = new UInt256((UTF8Encoding.UTF8.GetBytes("relaunch")).Sha256());

            UInt160 asset = UInt160.Parse("0xd74d35311c2a20ba78cd12056d3017da5bd352a6");
            string TokenId = "1LDil6dnxse4WiGC+2nk/gi0mnazuu0aMGI9hYsilHs=";
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                try
                {
                    sb.EmitDynamicCall(asset, "properties", Convert.FromBase64String(TokenId));

                }
                catch
                {
                    sb.EmitDynamicCall(asset, "properties", TokenId);
                }
                script = sb.ToArray();
            }
            var a = script.ToHexString();

            //var base64String = "7b226e616d65223a2247686f73744d61726b65742054657374204e4654222c226465736372697074696f6e223a224e6f74207265616c6c7920666f722073616c652c206e6f74206f726967696e616c20617274776f726b222c22696d616765223a22697066733a2f2f516d66527161414b6d53544153457a6f724234367145706236514a65656f784b6f3653566b4a7953363443767344222c22746f6b656e555249223a22222c2261747472696275746573223a5b7b2274797065223a22417274697374222c2276616c7565223a22556e6b6e6f776e222c22646973706c6179223a22227d2c7b2274797065223a224f726967696e616c222c2276616c7565223a224e6f7065222c22646973706c6179223a22227d2c7b2274797065223a22546573746e65742046756e222c2276616c7565223a22596573222c22646973706c6179223a22227d5d2c2270726f70657274696573223a7b226861735f6c6f636b6564223a747275652c2263726561746f72223a224e4c5a334b785864393838527633373473343231396877704d567175487841725944222c22726f79616c74696573223a323030302c2274797065223a317d7Q==";
            ////var base64String = "DCEC13y+vWO9KxAxFwg0SF0rjAJoq/n2N89uNwqrDwi+WHsMFIU5Il4pKR6Kf5xyOLaNS67/1PekEsAfDAR2b3RlDBT1Y+pAvCg9TQ4FxI6jBbPyoHNA70FifVtS";
            //var script = Convert.FromBase64String(base64String);
            //var str = Encoding.UTF8.GetString(script);
            //var scCalls = Neo.Plugins.VM.Helper.Script2ScCallModels(script, UInt256.Zero, UInt160.Zero, "");
        }

        [TestMethod]
        public void TestConvert()
        {
            //var a = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            //UInt160 from;
            //bool succ = UInt160.TryParse(Convert.FromBase64String("krOcd6pg8ptXwXPO2Rfxf9Mhpus=").Reverse().ToArray().ToHexString(), out from);
            //succ = UInt160.TryParse("d79888e16f9186e873b18a2bc80fd3ecf2ba74b3", out from);

            string properties = "{\"name\":\"CryptoFallen #6-19\",\"description\":\"CryptoFallen #6-19 \\\"Neo3 Series\\\" Follow @1Bigbagheera on twitter for updates!\",\"image\":\"ipfs://QmXZF4Pu1txhKo938X43RqrVJZ98p9QZyKnNGQ3ZR5Q3y3\",\"tokenURI\":\"\",\"attributes\":[{\"type\":\"Author\",\"value\":\"1Bigbagheera\",\"display\":\"\"},{\"type\":\"Date\",\"value\":\"8/25/21\",\"display\":\"\"},{\"type\":\"Series\",\"value\":\"Neo3\",\"display\":\"\"}],\"properties\":{\"has_locked\":false,\"creator\":\"NMV6PXumvk74JHkrrgynh932dQKd2p9vGF\",\"royalties\":1000,\"type\":2}}";
            Neo.Json.JObject jObject = (Neo.Json.JObject)Neo.Json.JObject.Parse(properties); 
        }

        public static string TryParseByteString(string str)
        {
            try
            {
                str = Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            catch
            {

            }

            return str;
        }

        public static void WriteArray(Utf8JsonWriter writer)
        {

        }

        public static void WriteObject(Utf8JsonWriter writer,string key, JsonElement element)
        {
            var type = element.GetProperty("type").GetString();
            string value = "";
            switch (type)
            {
                case "ByteString":
                    value = TryParseByteString(element.GetProperty("value").GetString());
                    writer.WriteString(key, value);
                    break;
                case "Integer":
                    value = element.GetProperty("value").GetString();
                    writer.WriteString(key, value);
                    break;
                case "Map":
                    var values = element.GetProperty("value");
                    foreach (var _e in values.EnumerateArray())
                    {
                        var _key = GetValue(_e.GetProperty("key"));
                        WriteObject(writer, _key, _e.GetProperty("value"));
                    }
                    break;
                case "Array":
                    writer.WriteStartObject(key);
                    foreach (var _e in element.GetProperty("value").EnumerateArray())
                    {
                        JsonElement jsonElement;
                        var suc = _e.TryGetProperty("key", out jsonElement);
                        if (suc)
                        {
                            var _key = GetValue(jsonElement);
                            WriteObject(writer, _key, _e.GetProperty("value"));
                        }
                        else
                        {
                            WriteObject(writer, "", _e);
                        }
                    }
                    writer.WriteEndObject();
                    break;
            }
        }

        public static string GetValue(JsonElement element)
        {
            var type = element.GetProperty("type").GetString();
            string value = "";
            switch (type)
            {
                case "ByteString":
                    value = TryParseByteString(element.GetProperty("value").GetString());
                    break;
                case "Integer":
                    value = element.GetProperty("value").GetString();
                    break;
            }
            return value;
        }

        [TestMethod]
        public void TestECPointParse()
        {
            var base64String = "CwwUaUQlwX8eu3xl3jAmyDHrTEnW174SwB8MBHZvdGUMFPVj6kC8KD1NDgXEjqMFs/Kgc0DvQWJ9W1I=";
            //var base64String = "DCEC13y+vWO9KxAxFwg0SF0rjAJoq/n2N89uNwqrDwi+WHsMFIU5Il4pKR6Kf5xyOLaNS67/1PekEsAfDAR2b3RlDBT1Y+pAvCg9TQ4FxI6jBbPyoHNA70FifVtS";
            var script = Convert.FromBase64String(base64String);
            var scCalls = Neo.Plugins.VM.Helper.Script2ScCallModels(script, UInt256.Zero, UInt160.Zero, "");
            UInt160 voter = null;
            bool succ = UInt160.TryParse(scCalls[0].HexStringParams[0].HexToBytes().Reverse().ToArray().ToHexString(), out voter);
            if (scCalls[0].HexStringParams[1] != string.Empty)
            {
                ECPoint ecPoint = null;
                succ = ECPoint.TryParse("", ECCurve.Secp256r1, out ecPoint);
                var candidate = Contract.CreateSignatureContract(ecPoint).ScriptHash;
            }
        }
    }
}

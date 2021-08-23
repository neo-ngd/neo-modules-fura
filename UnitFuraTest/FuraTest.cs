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
            var base64String = "EBAQExETDAAMAAwOUmVzcG9uc2liaWxpdHkMDURldGVybWluYXRpb24MBkxlYWRlcgwETWFsZQwKQ3J5cHRvbmF1dAyUVGhlIGZpcnN0IENyeXB0b25hdXQgdG8gbGFuZCBvbiB0aGUgbW9vbi4gLjAwMSBpcyB0aGUgcGlvbmVlciBvZiB0aGUgQ3J5cHRvbmF1dHMsIHdpdGggdGhlIHJlbGlhYmlsaXR5IGFuZCBkcml2ZSB0byBtYWludGFpbiBoaXMgaGVhZCBob25jaG8gc3RhdHVzLgxOaHR0cHM6Ly9taW50aW5nLXR0bS5zMy5hbWF6b25hd3MuY29tL05FVytNT09OKy0rQ1JZUFRPTkFVVFMvQ3J5cHRvbmF1dC4wMDEuc3ZnDANHSU4MDVNvdXRoIEFtZXJpY2EMCE5ldyBNb29uEQwETW9vbgwPQ3J5cHRvbmF1dCAuMDAxDARDMDAxDAlDaGFyYWN0ZXIAWgwUscULnROUsRBiD1tctANzvWBf6+MAGcAfDA1taW50Q2hhcmFjdGVyDBR8+ztrd+pH3tZmQRtewFdPJvVU5kFifVtS";
            //var base64String = "DCEC13y+vWO9KxAxFwg0SF0rjAJoq/n2N89uNwqrDwi+WHsMFIU5Il4pKR6Kf5xyOLaNS67/1PekEsAfDAR2b3RlDBT1Y+pAvCg9TQ4FxI6jBbPyoHNA70FifVtS";
            var script = Convert.FromBase64String(base64String);
            var scCalls = Neo.Plugins.VM.Helper.Script2ScCallModels(script,UInt256.Zero, UInt160.Zero);
        }

        [TestMethod]
        public void TestConvert()
        {
            var a = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            UInt160 from;
            bool succ = UInt160.TryParse(Convert.FromBase64String("krOcd6pg8ptXwXPO2Rfxf9Mhpus=").Reverse().ToArray().ToHexString(), out from);
            succ = UInt160.TryParse("d79888e16f9186e873b18a2bc80fd3ecf2ba74b3", out from);
        }

        [TestMethod]
        public void TestECPointParse()
        {
            var base64String = "CwwUaUQlwX8eu3xl3jAmyDHrTEnW174SwB8MBHZvdGUMFPVj6kC8KD1NDgXEjqMFs/Kgc0DvQWJ9W1I=";
            //var base64String = "DCEC13y+vWO9KxAxFwg0SF0rjAJoq/n2N89uNwqrDwi+WHsMFIU5Il4pKR6Kf5xyOLaNS67/1PekEsAfDAR2b3RlDBT1Y+pAvCg9TQ4FxI6jBbPyoHNA70FifVtS";
            var script = Convert.FromBase64String(base64String);
            var scCalls = Neo.Plugins.VM.Helper.Script2ScCallModels(script, UInt256.Zero, UInt160.Zero);
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

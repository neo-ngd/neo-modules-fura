using System;
using System.Numerics;
using System.Reflection;
using Neo;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.Plugins.Notification;
using Neo.VM.Types;

namespace UnitFuraTest
{
    /// <summary>
    /// Reflection helpers so tests do not require changes to Fura.csproj (e.g. InternalsVisibleTo).
    /// </summary>
    internal static class FuraTestAccess
    {
        private static readonly Type s_notificationMgr = typeof(NotificationMgr);
        private static readonly Type s_cacheGasMintBurn = typeof(CacheGasMintBurn);
        private static readonly Type s_candidateState = typeof(NotificationMgr).Assembly.GetType("Neo.Plugins.VM.CandidateState")!;

        private static readonly MethodInfo s_isNullStackItem = s_notificationMgr.GetMethod(
            "IsNullStackItem", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo s_tryParseBase64 = s_notificationMgr.GetMethod(
            "TryParseBase64ToScriptHash", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo s_tryParseNotificationHash = s_notificationMgr.GetMethod(
            "TryParseNotificationHash", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo s_getPreviousTotals = s_cacheGasMintBurn.GetMethod(
            "GetPreviousTotals", BindingFlags.NonPublic | BindingFlags.Static)!;

        public static bool IsNullStackItem(NotificationStateValueModel value) =>
            (bool)s_isNullStackItem.Invoke(null, new object[] { value })!;

        public static bool TryParseBase64ToScriptHash(string base64String, out UInt160 addr)
        {
            object[] args = { base64String, null };
            var result = (bool)s_tryParseBase64.Invoke(null, args)!;
            addr = (UInt160)args[1];
            return result;
        }

        public static bool TryParseNotificationHash(NotificationStateValueModel value, out UInt160 hash)
        {
            object[] args = { value, null };
            var result = (bool)s_tryParseNotificationHash.Invoke(null, args)!;
            hash = (UInt160)args[1];
            return result;
        }

        public static (BigInteger TotalBurn, BigInteger TotalMint) GetPreviousTotals(GasMintBurnModel previous) =>
            ((BigInteger, BigInteger))s_getPreviousTotals.Invoke(null, new object[] { previous })!;

        public static object CreateCandidateState(bool registered, BigInteger votes)
        {
            var state = Activator.CreateInstance(s_candidateState)!;
            s_candidateState.GetField("Registered")!.SetValue(state, registered);
            s_candidateState.GetField("Votes")!.SetValue(state, votes);
            return state;
        }

        public static StackItem CandidateStateToStackItem(object state) =>
            (StackItem)s_candidateState.GetMethod("ToStackItem")!.Invoke(state, null)!;

        public static void CandidateStateFromStackItem(object state, StackItem item) =>
            s_candidateState.GetMethod("FromStackItem")!.Invoke(state, new object[] { item });

        public static bool GetCandidateRegistered(object state) =>
            (bool)s_candidateState.GetField("Registered")!.GetValue(state)!;

        public static BigInteger GetCandidateVotes(object state) =>
            (BigInteger)s_candidateState.GetField("Votes")!.GetValue(state)!;
    }
}

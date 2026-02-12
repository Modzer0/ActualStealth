using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearOptionRCSMod
{
    [BepInPlugin("com.nuclearoption.rcsmod", "Nuclear Option RCS Mod", "0.1.0-beta")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<float> VortexDivisor;
        internal static ConfigEntry<float> IfritDivisor;
        internal static ConfigEntry<float> DarkreachDivisor;

        private void Awake()
        {
            Log = Logger;

            VortexDivisor = Config.Bind("RCS Divisors", "Vortex", 100f,
                "RCS divisor for the FS-20B Vortex (SmallFighter1)");
            IfritDivisor = Config.Bind("RCS Divisors", "Ifrit", 100f,
                "RCS divisor for the KR-67A Ifrit (Multirole1)");
            DarkreachDivisor = Config.Bind("RCS Divisors", "Darkreach", 100f,
                "RCS divisor for the SFB-81 Darkreach");

            new Harmony("com.nuclearoption.rcsmod").PatchAll();
            Logger.LogInfo("RCS Mod loaded");
        }
    }

    [HarmonyPatch(typeof(Unit), "Awake")]
    public static class UnitAwakePatch
    {
        private static readonly Dictionary<string, System.Func<float>> _divisorLookup =
            new Dictionary<string, System.Func<float>>
        {
            { "SmallFighter1", () => Plugin.VortexDivisor.Value },
            { "Multirole1",    () => Plugin.IfritDivisor.Value },
            { "Darkreach",     () => Plugin.DarkreachDivisor.Value }
        };

        [HarmonyPostfix]
        public static void Postfix(Unit __instance)
        {
            string name = __instance.gameObject.name.Replace("(Clone)", "").Trim();

            if (_divisorLookup.TryGetValue(name, out var getDivisor))
            {
                float divisor = getDivisor();
                if (divisor <= 0f) return;

                float originalRCS = __instance.RCS;
                __instance.RCS = originalRCS / divisor;
                Plugin.Log.LogInfo($"[RCS Mod] {name}: RCS {originalRCS} -> {__instance.RCS} (divisor: {divisor})");
            }
        }
    }
}

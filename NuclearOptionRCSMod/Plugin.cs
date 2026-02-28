using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NuclearOptionRCSMod
{
    [BepInPlugin("com.nuclearoption.rcsmod", "Nuclear Option RCS Mod", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<float> VortexDivisor;
        internal static ConfigEntry<float> IfritDivisor;
        internal static ConfigEntry<float> DarkreachDivisor;
        internal static ConfigEntry<float> MedusaRadarMultiplier;

        private void Awake()
        {
            Log = Logger;

            VortexDivisor = Config.Bind("RCS Divisors", "Vortex", 100f,
                "RCS divisor for the FS-20B Vortex (SmallFighter1)");
            IfritDivisor = Config.Bind("RCS Divisors", "Ifrit", 100f,
                "RCS divisor for the KR-67A Ifrit (Multirole1)");
            DarkreachDivisor = Config.Bind("RCS Divisors", "Darkreach", 100f,
                "RCS divisor for the SFB-81 Darkreach");

            MedusaRadarMultiplier = Config.Bind("Radar", "MedusaRadarMultiplier", 2f,
                "Multiplier for the Medusa (EW1) radome signal strength (applied to maxRange)");

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

    [HarmonyPatch(typeof(Radar), "AttachToUnit")]
    public static class RadarAttachPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Radar __instance, Unit unit)
        {
            if (unit == null) return;

            string unitName = unit.gameObject.name.Replace("(Clone)", "").Trim();
            if (unitName != "EW1") return;

            // Only boost the radome equipment, not any built-in radar
            string radarObjName = __instance.gameObject.name;
            if (!radarObjName.Contains("Radome")) return;

            float multiplier = Plugin.MedusaRadarMultiplier.Value;
            if (multiplier <= 0f || multiplier == 1f) return;

            float originalRange = __instance.RadarParameters.maxRange;
            __instance.RadarParameters.maxRange *= multiplier;
            Plugin.Log.LogInfo($"[RCS Mod] Medusa radome ({radarObjName}): maxRange {originalRange} -> {__instance.RadarParameters.maxRange} (x{multiplier})");
        }
    }

    [HarmonyPatch(typeof(HintsTipsDisplay), "ReadHints")]
    public static class HintsPatch
    {
        private static readonly string[] _customHints = new string[]
        {
            "Wide Tom's Bean House is known worldwide for it's variety of bean cuisine. The only complaints its ever recieved are for constant flatulance in the patrons.",
            "Move freight! Miners don't die! - Randolph P Checkers Esq., Quartermaster.",
            "It's never a warcrime the first time...",
            "Guinness Book of World Records has Tom Tombadil's home stove as the widest home stovetop in existance.",
            "The Black Pants Legion is not a cult, or a militia."
        };

        [HarmonyPostfix]
        public static void Postfix(HintsTipsDisplay __instance)
        {
            var listField = typeof(HintsTipsDisplay).GetField("listHints", BindingFlags.NonPublic | BindingFlags.Instance);
            if (listField == null) return;

            var list = listField.GetValue(__instance) as IList;
            if (list == null) return;

            // HintTip is a private struct, so we use reflection to create instances
            var hintTipType = typeof(HintsTipsDisplay).GetNestedType("HintTip", BindingFlags.NonPublic);
            if (hintTipType == null) return;

            var idField = hintTipType.GetField("id");
            var typeField = hintTipType.GetField("type");
            var textField = hintTipType.GetField("text");

            foreach (var hint in _customHints)
            {
                var entry = System.Activator.CreateInstance(hintTipType);
                idField.SetValue(entry, list.Count + 1);
                typeField.SetValue(entry, "Did you know?");
                textField.SetValue(entry, hint);
                list.Add(entry);
            }

            Plugin.Log.LogInfo($"[RCS Mod] Added {_customHints.Length} custom hints");
        }
    }
}

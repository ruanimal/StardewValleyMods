using System;
using HarmonyLib;

namespace FilteredChestHopper
{
    internal class Patcher
    {
        private static Action<StardewValley.Object> OnMachineMinutesElapsed;

        public Patcher(Action<StardewValley.Object> onMachineMinutesElapsed)
        {
            OnMachineMinutesElapsed = onMachineMinutesElapsed;
        }

        internal static void Patch(string id)
        {
            Harmony harmony = new(id);
            harmony.PatchAll();
            var original = AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.minutesElapsed), null, null);
            var post = new HarmonyMethod(AccessTools.Method(typeof(Patcher), nameof(OnMinutesElapsed)));
            harmony.Patch(original, post);
        }

        private static void OnMinutesElapsed(StardewValley.Object __instance)
        {
            OnMachineMinutesElapsed(__instance);
        }
    }
}

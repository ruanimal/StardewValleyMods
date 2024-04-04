using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using HarmonyLib;

namespace FilteredHopper
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc/>
        public override void Entry(IModHelper helper)
        {
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll();
            harmony.Patch(original: AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed), null, null), postfix: new HarmonyMethod(AccessTools.Method(this.GetType(), nameof(OnMachineMinutesElapsed))));
        }

        /// <summary>Called after a machine updates on time change.</summary>
        /// <param name="machine">The machine that updated.</param>
        /// <param name="location">The location containing the machine.</param>
        private void OnMachineMinutesElapsed(SObject machine, GameLocation location)
        {
            // not hopper
            if (!this.TryGetHopper(machine, out Chest hopper))
                return;

            // check for bottom chest
            if (!location.objects.TryGetValue(hopper.TileLocation + new Vector2(0, 1), out SObject objBelow) || objBelow is not Chest chestBelow)
                return;

            // check for top chest
            if (!location.objects.TryGetValue(hopper.TileLocation - new Vector2(0, 1), out SObject objAbove) || objAbove is not Chest chestAbove)
                return;


            // transfer items
            chestAbove.clearNulls();
            var chestAboveItems = chestAbove.GetItemsForPlayer(hopper.owner.Value);
            var filterItems = hopper.GetItemsForPlayer(hopper.owner.Value);
            for (int i = chestAboveItems.Count - 1; i >= 0; i--)
            {
                bool match = true;
                for (int j = filterItems.Count - 1; j >= 0; j--)
                {
                    if (filterItems[j].ItemId == chestAboveItems[i].ItemId)
                    {
                        match = true;
                        break;
                    }
                    else
                    {
                        match = false;
                    }
                }
                if(match)
                {
                    Item item = chestAboveItems[i];
                    if (chestBelow.addItem(item) == null)
                        chestAboveItems.RemoveAt(i);
                }
            }
        }

        /// <summary>Get the hopper instance if the object is a hopper.</summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="hopper">The hopper instance.</param>
        /// <returns>Returns whether the object is a hopper.</returns>
        private bool TryGetHopper(SObject obj, out Chest hopper)
        {
            if (obj is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest)
            {
                hopper = chest;
                return true;
            }

            hopper = null;
            return false;
        }
    }
}

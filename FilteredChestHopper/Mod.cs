using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace FilteredChestHopper
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            new Patcher(this.OnMinutesElapsed);
            Patcher.Patch(ModManifest.UniqueID);
        }

        private void OnMinutesElapsed(StardewValley.Object __instance)
        {
            // not hopper
            if (!this.TryGetHopper(__instance, out Chest hopper))
            return;

            // check for bottom chest
            if (!__instance.Location.objects.TryGetValue(hopper.TileLocation + new Vector2(0, 1), out StardewValley.Object objBelow) || objBelow is not Chest chestBelow)
                return;

            // check for top chest
            if (!__instance.Location.objects.TryGetValue(hopper.TileLocation - new Vector2(0, 1), out StardewValley.Object objAbove) || objAbove is not Chest chestAbove)
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
        private bool TryGetHopper(StardewValley.Object obj, out Chest hopper)
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

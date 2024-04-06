using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace FilteredChestHopper
{
    internal class Pipeline
    {
        //position of the left most hopper
        internal Vector2 Position;
        //width of the pipeline
        internal int Width;
        //location
        internal GameLocation Location;

        public Pipeline(Chest originHopper)
        {
            Position = originHopper.TileLocation;
            Width = 1;
            Location = originHopper.Location;

            originHopper.modData[Mod.ModDataFlag] = "1";

            CheckSideHoppers(new Vector2(1, 0), originHopper);
            CheckSideHoppers(new Vector2(-1, 0), originHopper);
        }

        //Checks adjacent hoppers for expansion
        private void CheckSideHoppers(Vector2 direction, Chest hopper)
        {
            //check for hopper in direction
            Chest chest = Mod.GetChestAt(Location, hopper.TileLocation + direction);
            if (chest == null || !Mod.TryGetHopper(chest, out hopper))
            {
                return;
            }

            ExpandPipeline(hopper);

            CheckSideHoppers(direction, hopper);
        }

        internal void ExpandPipeline(Chest hopper)
        {
            //Expand Pipeline
            if(hopper.TileLocation.X < Position.X)
            {
                Position = hopper.TileLocation;
            }
            Width += 1;

            hopper.modData[Mod.ModDataFlag] = "1";
        }

        //Attempt to output with this hopper as a filter
        public void AttemptTransfer()
        {
            List<Chest> inputChests = new List<Chest>();
            List<Chest[]> outputChests = new List<Chest[]>();
            for (int i = 0; i < Width; i++)
            {
                Chest inputChest = Mod.GetChestAt(Location, Position + new Vector2(i, -1));
                if (inputChest != null)
                {
                    inputChests.Add(inputChest);
                }

                Chest outputChest = Mod.GetChestAt(Location, Position + new Vector2(i, 1));
                if (outputChest != null)
                {
                    outputChests.Add(new Chest[] { Mod.GetChestAt(Location, Position + new Vector2(i, 0)), outputChest});
                }
            }

            foreach (var inputChest in inputChests)
            {
                inputChest.clearNulls();
                var chestAboveItems = inputChest.GetItemsForPlayer(inputChest.owner.Value);
                foreach (var outputChest in outputChests)
                {
                    var filterItems = outputChest[0].GetItemsForPlayer(inputChest.owner.Value);
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
                        if (match)
                        {
                            Item item = chestAboveItems[i];
                            if (outputChest[1].addItem(item) == null)
                                chestAboveItems.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}

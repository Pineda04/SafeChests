using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

namespace SafeChests
{
    public class ChestProtectionSystem : ModSystem
    {
        public static HashSet<Point> ProtectedChests = new();

        public static void ToggleChestProtection(int x, int y)
        {
            Point chestPos = new(x, y);
            if (ProtectedChests.Contains(chestPos))
                ProtectedChests.Remove(chestPos);
            else
                ProtectedChests.Add(chestPos);
        }

        public static bool IsChestProtected(int x, int y) => ProtectedChests.Contains(new Point(x, y));

        public override void SaveWorldData(TagCompound tag)
        {
            List<int> data = new();
            foreach (var pos in ProtectedChests)
            {
                data.Add(pos.X);
                data.Add(pos.Y);
            }
            tag["ProtectedChests"] = data;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ProtectedChests.Clear();
            if (tag.ContainsKey("ProtectedChests"))
            {
                var data = tag.Get<List<int>>("ProtectedChests");
                for (int i = 0; i < data.Count; i += 2)
                {
                    ProtectedChests.Add(new Point(data[i], data[i + 1]));
                }
            }
        }
    }
}
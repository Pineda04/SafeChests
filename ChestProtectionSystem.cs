using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SafeChests
{
    public class ChestProtectionSystem : ModSystem
    {
        public static Dictionary<Point, string> ProtectedChests = new();

        public static void ToggleChestProtection(int x, int y, string password = "")
        {
            Point chestPos = new(x, y);
            if (ProtectedChests.ContainsKey(chestPos))
                ProtectedChests.Remove(chestPos);
            else
                ProtectedChests.Add(chestPos, password);
        }

        public static bool IsChestProtected(int x, int y) => ProtectedChests.ContainsKey(new Point(x, y));

        public static string GetChestPassword(int x, int y) => ProtectedChests.TryGetValue(new Point(x, y), out string password) ? password : "";

        public override void SaveWorldData(TagCompound tag)
        {
            List<int> positions = new();
            List<string> passwords = new();
            foreach (var pair in ProtectedChests)
            {
                positions.Add(pair.Key.X);
                positions.Add(pair.Key.Y);
                passwords.Add(pair.Value);
            }
            tag["ProtectedChests"] = positions;
            tag["ProtectedChestsPassword"] = passwords;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ProtectedChests.Clear();
            if (tag.ContainsKey("ProtectedChests"))
            {
                var positions = tag.Get<List<int>>("ProtectedChests");
                var passwords = tag.Get<List<string>>("ProtectedChestsPassword");
                for (int i = 0; i < positions.Count; i += 2)
                {
                    ProtectedChests.Add(new Point(positions[i], positions[i + 1]), passwords[i / 2]);
                }
            }
        }
    }
}
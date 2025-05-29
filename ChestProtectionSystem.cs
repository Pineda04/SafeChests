using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SafeChests
{
    public class ChestProtectionSystem : ModSystem
    {
        public static Dictionary<Point, string> ProtectedChests = new();

        public static void ToggleChestProtection(int x, int y, string password, int whoAmI = -1)
        {
            Point chestPos = new(x, y);
            bool wasProtected = ProtectedChests.ContainsKey(chestPos);
            string action = "";

            if (wasProtected)
            {
                ProtectedChests.Remove(chestPos);
                action = "desprotegido";
            }
            else
            {
                ProtectedChests.Add(chestPos, password);
                action = "protegido";
            }

            // Enviar mensaje a todos los jugadores
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                SendChestUpdate(x, y, password, wasProtected);
                // SendMessageToAll($"Cofre en ({x}, {y}) {action}.", Color.LightBlue, whoAmI);
            }
            else
            {
                // Main.NewText($"Cofre en ({x}, {y}) {action}.", Color.LightBlue);
            }
        }

        public static bool IsChestProtected(int x, int y) => ProtectedChests.ContainsKey(new Point(x, y));

        public static string GetChestPassword(int x, int y) => ProtectedChests.TryGetValue(new Point(x, y), out string password) ? password : "";

        public static void SendChestUpdate(int x, int y, string password, bool wasProtected)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = ModContent.GetInstance<SafeChests>().GetPacket();
            packet.Write((byte)0); // Identificador del tipo de paquete
            packet.Write(x);
            packet.Write(y);
            packet.Write(password);
            packet.Write(wasProtected);
            packet.Send();
        }

        public static void SendMessageToAll(string message, Color color, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(message, color);
            }
            else
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral(message), color, ignoreClient);
            }
        }

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
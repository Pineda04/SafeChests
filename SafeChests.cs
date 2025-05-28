using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SafeChests
{
    public class SafeChests : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte packetType = reader.ReadByte();
            if (packetType == 0)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                string password = reader.ReadString();
                bool wasProtected = reader.ReadBoolean();

                if (Main.netMode == NetmodeID.Server)
                {
                    // El servidor valida y actualiza
                    ChestProtectionSystem.ToggleChestProtection(x, y, password, whoAmI);
                }
                else
                {
                    // Los clientes actualizan su estado local
                    Point chestPos = new(x, y);
                    if (wasProtected)
                    {
                        ChestProtectionSystem.ProtectedChests.Remove(chestPos);
                    }
                    else
                    {
                        ChestProtectionSystem.ProtectedChests.Add(chestPos, password);
                    }

                    // Actualizar la UI si el cofre afectado está abierto
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        Player player = Main.player[Main.myPlayer];
                        if (player.chest != -1)
                        {
                            Chest chest = Main.chest[player.chest];
                            if (chest != null && chest.x == x && chest.y == y)
                            {
                                // Forzar actualización de la UI
                                var chestUISystem = ModContent.GetInstance<ChestUISystem>();
                                if (chestUISystem != null)
                                {
                                    chestUISystem.UpdateChestUI();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
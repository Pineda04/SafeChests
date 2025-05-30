using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace SafeChests
{
    public class SafeChests : Mod
    {
        public override void PostSetupContent()
        {
            // Detectar el idioma actual para debug
            if (Main.netMode != NetmodeID.Server)
            {
                string currentLanguage = Language.ActiveCulture.Name;
                Logger.Info($"Idioma detectado: {currentLanguage}");
                Logger.Info($"Texto de prueba: {Language.GetTextValue("Mods.SafeChests.UI.ProtectChest")}");
            }
        }

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
                    ChestProtectionSystem.ToggleChestProtection(x, y, password, whoAmI);
                }
                else
                {
                    Point chestPos = new(x, y);
                    if (wasProtected)
                    {
                        ChestProtectionSystem.ProtectedChests.Remove(chestPos);
                    }
                    else
                    {
                        ChestProtectionSystem.ProtectedChests.Add(chestPos, password);
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        Player player = Main.player[Main.myPlayer];
                        if (player.chest != -1)
                        {
                            Chest chest = Main.chest[player.chest];
                            if (chest != null && chest.x == x && chest.y == y)
                            {
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

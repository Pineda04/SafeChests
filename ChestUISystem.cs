using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Localization;
using SafeChests.UI;

namespace SafeChests
{
    public class ChestUISystem : ModSystem
    {
        private UserInterface _chestInterface;
        private ChestButtonUI _chestButtonUI;

        // Método helper para obtener texto localizado con fallback
        private string GetLocalizedText(string key, string fallback = "")
        {
            try
            {
                var text = Language.GetTextValue(key);
                if (text == key && !string.IsNullOrEmpty(fallback))
                {
                    return fallback;
                }
                return text;
            }
            catch
            {
                return !string.IsNullOrEmpty(fallback) ? fallback : key;
            }
        }

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                _chestInterface = new UserInterface();
                _chestButtonUI = new ChestButtonUI();
                _chestInterface?.SetState(_chestButtonUI);
                ChestProtectionSystem.SendMessageToAll(
                    GetLocalizedText("Mods.SafeChests.Messages.UIInitialized", "SafeChests: UI inicializada"),
                    Color.Green);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.netMode != NetmodeID.Server && Main.playerInventory && IsRealChest())
            {
                _chestInterface?.Update(gameTime);
            }
        }

        private bool IsRealChest()
        {
            Player player = Main.player[Main.myPlayer];

            if (player.chest == -1) return false;
            if (player.chest == -2) return false; // Hucha
            if (player.chest == -3) return false; // Caja fuerte
            if (player.chest == -4) return false; // Cofre del Vacío
            if (player.chest == -5) return false; // Cofre de Equipo

            Chest chest = Main.chest[player.chest];
            return chest != null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                if (inventoryIndex != -1)
                {
                    layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                        "SafeChests: Chest UI",
                        delegate
                        {
                            if (Main.playerInventory && IsRealChest())
                            {
                                Player player = Main.player[Main.myPlayer];
                                Chest chest = Main.chest[player.chest];
                                if (chest != null)
                                {
                                    bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                                    _chestInterface.Draw(Main.spriteBatch, new GameTime());
                                    Main.instance.invBottom = isLocked ? 1000 : 258;
                                }
                            }
                            else if (Main.playerInventory && Main.player[Main.myPlayer].chest != -1 && !IsRealChest())
                            {
                                Main.instance.invBottom = 258;
                            }
                            return true;
                        },
                        InterfaceScaleType.UI));
                }
            }
        }

        public void UpdateChestUI()
        {
            if (Main.netMode != NetmodeID.Server && _chestButtonUI != null)
            {
                Player player = Main.player[Main.myPlayer];
                if (IsRealChest())
                {
                    Chest chest = Main.chest[player.chest];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        string buttonText = isLocked ?
                            GetLocalizedText("Mods.SafeChests.UI.UnlockChest", "Desbloquear cofre") :
                            GetLocalizedText("Mods.SafeChests.UI.ProtectChest", "Proteger cofre");
                        _chestButtonUI.UpdateButtonText(buttonText);
                    }
                }
            }
        }
    }
}

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.DataStructures;
using SafeChests.UI;

namespace SafeChests
{
    public class ChestUISystem : ModSystem
    {
        private UserInterface _chestInterface;
        private ChestButtonUI _chestButtonUI;

        public override void Load()
        {
            _chestInterface = new UserInterface();
            _chestButtonUI = new ChestButtonUI();
            _chestInterface.SetState(_chestButtonUI);
            Main.NewText("SafeChests: UI inicializada", Color.Green);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.playerInventory && Main.player[Main.myPlayer].chest != -1)
            {
                _chestInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "SafeChests: Chest UI",
                    delegate
                    {
                        if (Main.playerInventory && Main.player[Main.myPlayer].chest != -1)
                        {
                            _chestInterface.Draw(Main.spriteBatch, new GameTime());
                            // Ocultar ítems si el cofre está protegido
                            if (ChestProtectionSystem.IsChestProtected(Main.chest[Main.player[Main.myPlayer].chest].x, Main.chest[Main.player[Main.myPlayer].chest].y))
                            {
                                Main.instance.invBottom = 1000; // Oculta los ítems
                            }
                            else
                            {
                                Main.instance.invBottom = 258; // Posición normal de la UI
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
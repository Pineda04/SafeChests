using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
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
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}

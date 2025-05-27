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
                            Chest chest = Main.chest[Main.player[Main.myPlayer].chest];
                            if (chest != null)
                            {
                                bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                                // Dibujar siempre la interfaz de usuario personalizada
                                _chestInterface.Draw(Main.spriteBatch, new GameTime());
                                // Ocultar objetos del inventario si el cofre está protegido
                                if (isLocked)
                                {
                                    Main.instance.invBottom = 1000; // Ocultar los items
                                }
                                else
                                {
                                    Main.instance.invBottom = 258; // Mostrar los items
                                }
                            }
                            else
                            {
                                // Mostrar inventario normal si el cofre es nulo
                                Main.instance.invBottom = 258;
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
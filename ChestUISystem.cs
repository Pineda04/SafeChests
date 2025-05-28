using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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
            // Solo inicializar la UI en el cliente, no en el servidor
            if (Main.netMode != NetmodeID.Server)
            {
                _chestInterface = new UserInterface();
                _chestButtonUI = new ChestButtonUI();
                _chestInterface?.SetState(_chestButtonUI);
                ChestProtectionSystem.SendMessageToAll("SafeChests: UI inicializada", Color.Green);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // Solo actualizar la UI en el cliente
            if (Main.netMode != NetmodeID.Server && Main.playerInventory && Main.player[Main.myPlayer].chest != -1)
            {
                _chestInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Solo modificar las capas de la interfaz en el cliente
            if (Main.netMode != NetmodeID.Server)
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
                                    Main.instance.invBottom = isLocked ? 1000 : 258;
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

        public void UpdateChestUI()
        {
            if (Main.netMode != NetmodeID.Server && _chestButtonUI != null)
            {
                Player player = Main.player[Main.myPlayer];
                if (player.chest != -1)
                {
                    Chest chest = Main.chest[player.chest];
                    if (chest != null)
                    {
                        bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);
                        _chestButtonUI.UpdateButtonText(isLocked ? "Desbloquear cofre" : "Proteger cofre");
                    }
                }
            }
        }
    }
}
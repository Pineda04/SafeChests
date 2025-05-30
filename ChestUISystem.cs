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
            // Solo actualizar la UI en el cliente y solo para cofres reales
            if (Main.netMode != NetmodeID.Server && Main.playerInventory && IsRealChest())
            {
                _chestInterface?.Update(gameTime);
            }
        }

        // Método para verificar si el contenedor abierto es un cofre real
        private bool IsRealChest()
        {
            Player player = Main.player[Main.myPlayer];
            
            // Verificar si hay un cofre abierto
            if (player.chest == -1) return false;
            
            // Verificar si no es hucha, caja fuerte, cofre del vacío, cofre de equipo, etc.
            if (player.chest == -2) return false; // Hucha
            if (player.chest == -3) return false; // Caja fuerte
            if (player.chest == -4) return false; // Cofre del Vacío
            if (player.chest == -5) return false; // Cofre de Equipo
            
            // Si el índice es positivo, es un cofre real en el mundo
            Chest chest = Main.chest[player.chest];
            return chest != null;
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
                            if (Main.playerInventory && IsRealChest())
                            {
                                Player player = Main.player[Main.myPlayer];
                                Chest chest = Main.chest[player.chest];
                                if (chest != null)
                                {
                                    bool isLocked = ChestProtectionSystem.IsChestProtected(chest.x, chest.y);

                                    // Dibujar siempre la interfaz de usuario personalizada para cofres reales
                                    _chestInterface.Draw(Main.spriteBatch, new GameTime());
                                    Main.instance.invBottom = isLocked ? 1000 : 258;
                                }
                            }
                            else if (Main.playerInventory && Main.player[Main.myPlayer].chest != -1 && !IsRealChest())
                            {
                                // Para contenedores especiales (hucha, caja fuerte, etc.), usar comportamiento normal
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
                        _chestButtonUI.UpdateButtonText(isLocked ? "Desbloquear cofre" : "Proteger cofre");
                    }
                }
            }
        }
    }
}
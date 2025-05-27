using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SafeChests.UI
{
    internal class ChestButtonUI : UIState
    {
        private UITextPanel<string> protectButton;
        private bool isLocked = false;

        public override void OnInitialize()
        {
            protectButton = new UITextPanel<string>("Proteger cofre", 0.8f, false)
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 30 },
                Left = { Pixels = 73 },
                Top = { Pixels = 428 }
            };

            protectButton.OnMouseOver += (evt, element) =>
                protectButton.BorderColor = Color.Yellow;

            protectButton.OnMouseOut += (evt, element) =>
                protectButton.BorderColor = Color.Black;

            protectButton.OnLeftClick += (evt, element) =>
            {
                Player player = Main.player[Main.myPlayer];
                int chestIndex = player.chest;

                if (chestIndex != -1)
                {
                    Chest chest = Main.chest[chestIndex];
                    if (chest != null)
                    {
                        isLocked = !isLocked;
                        ChestProtectionSystem.ToggleChestProtection(chest.x, chest.y);
                        protectButton.SetText(isLocked ? "Desbloquear cofre" : "Proteger cofre");

                        string action = isLocked ? "protegido" : "desprotegido";
                        Main.NewText($"Cofre en ({chest.x}, {chest.y}) {action}.", Color.LightBlue);
                    }
                }
                else
                {
                    Main.NewText("No hay ningún cofre abierto.", Color.Red);
                }
            };

            Append(protectButton);
        }
    }
}

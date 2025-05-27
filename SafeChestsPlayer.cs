using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SafeChests
{
    public class SafeChestsPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.chest != -1)
            {
                Chest chest = Main.chest[Player.chest];
                if (chest != null && ChestProtectionSystem.IsChestProtected(chest.x, chest.y))
                {
                    Main.NewText("Este cofre está protegido con SafeChests.", Color.Red);
                    // No cerramos el cofre, permitimos que la UI permanezca abierta
                }
            }
        }
    }
}
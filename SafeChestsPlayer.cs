using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
                    Main.NewText("Este cofre está protegido.", Color.Red);

                    // Cerrar el cofre manualmente
                    Main.playerInventory = false;
                    Main.recBigList = false;
                    Main.player[Main.myPlayer].chest = -1;
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
            }
        }
    }
}

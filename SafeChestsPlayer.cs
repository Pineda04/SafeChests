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
            }
        }
    }
}
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ItemCustomizer
{
	public class CustomizerPlayer : ModPlayer
	{
		public override void PreUpdate()
		{
			if(Main.myPlayer != player.whoAmI) {
				return;
			}

			Item held;
			if(Main.mouseItem.active && !Main.mouseItem.IsAir && Main.mouseItem.type > 0) {
				held = Main.mouseItem;
			} else {
				held = player.inventory[player.selectedItem];
			}
			if(!held.active || held.IsAir) {
				(mod as CustomizerMod).heldShaders[player.whoAmI] = 0;
			} else {
				CustomizerItem info = held.GetGlobalItem<CustomizerItem>(mod);
				(mod as CustomizerMod).heldShaders[player.whoAmI] = info.shaderID;
			}

			if(Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket pak = mod.GetPacket();
				pak.Write(CustomizerMod.pakCheckString);
				pak.Write((mod as CustomizerMod).heldShaders[Main.myPlayer]);
				pak.Send();
			}
		}

		public override bool PreItemCheck()
		{
			//Main.NewText("Clearing list");
			CustomizerProjectile.newProjectiles = new List<int>();
			return true;
		}

		public override void PostItemCheck()
		{
			//Main.NewText("PostItemCheck doing its thing? " + CustomizerProjectile.newProjectiles.Count + " in list");
			if(CustomizerMod.mod.heldShaders[Main.myPlayer] > 0) {
				foreach(int proj in CustomizerProjectile.newProjectiles) {
					//Main.NewText("Applying shader to projectile " + proj);
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetGlobalProjectile<CustomizerProjInfo>(mod);
					shotInfo.shaderID = CustomizerMod.mod.heldShaders[Main.myPlayer];
				}
				CustomizerProjectile.newProjectiles = new List<int>();
			}
		}
	}
}

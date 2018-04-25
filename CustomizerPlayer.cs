using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ShaderLib;

namespace ItemCustomizer
{
	public class CustomizerPlayer : ModPlayer
	{
		//public int ammoShader = 0;

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
				(mod as CustomizerMod).heldShaders[player.whoAmI] = new ShaderID(-1);
			} else {
				CustomizerItem info = held.GetGlobalItem<CustomizerItem>(mod);
				(mod as CustomizerMod).heldShaders[player.whoAmI] = info.shaderID;
			}

			CustomizerMod.mod.SendHeldShaderPacket();
		}

		public override bool PreItemCheck()
		{
			CustomizerMod.mod.ammoShaders[Main.myPlayer] = new ShaderID(-1);
			CustomizerProjectile.newProjectiles = new List<int>();
			return true;
		}

		public override void PostItemCheck()
		{
			if(CustomizerMod.mod.heldShaders[player.whoAmI].ID > 0 || CustomizerMod.mod.ammoShaders[player.whoAmI].ID > 0) {
				foreach(int proj in CustomizerProjectile.newProjectiles) {
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetGlobalProjectile<CustomizerProjInfo>(mod);

					//Yes, there's a specific thing for the Vortex Beater. It's the only ammo-using weapon I know that displays as a projectile.
					if(Main.projectile[proj].type == ProjectileID.VortexBeater || CustomizerMod.mod.ammoShaders[player.whoAmI].ID <= 0) {
						shotInfo.shaderID = CustomizerMod.mod.heldShaders[player.whoAmI].ID;
					} else {
						shotInfo.shaderID = CustomizerMod.mod.ammoShaders[player.whoAmI].ID;
					}
				}

				CustomizerProjectile.newProjectiles = new List<int>();
			}
		}
	}
}

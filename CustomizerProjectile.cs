using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerProjectile : GlobalProjectile
	{
		public static List<int> newProjectiles = new List<int>();

		public override void SetDefaults(Projectile projectile) {
			newProjectiles.Add(projectile.whoAmI);
		}

		public override bool PreAI(Projectile projectile) {
			CustomizerProjInfo projInfo = projectile.GetGlobalProjectile<CustomizerProjInfo>(mod);

			bool hook = Main.projHook[projectile.type];
			bool pet = (projectile.type == ProjectileID.StardustGuardian) || (Main.projPet[projectile.type] && !projectile.minion && projectile.damage == 0 && !ProjectileID.Sets.LightPet[projectile.type]);
			bool lightPet = !projectile.minion && projectile.damage == 0 && ProjectileID.Sets.LightPet[projectile.type];

			//Apply shader of the player's held item provided it meets all this criteria to make absolute certain a player fired it
			//As of 1.1.3 this now only applies to OTHER PLAYER's projectiles! Until I can get the better system working with multiplayer, that is.
			if(projInfo.parent && !(hook || pet || lightPet) && !projectile.npcProj && !projectile.trap && projectile.owner != 255 && projectile.owner != Main.myPlayer && Main.player[projectile.owner].itemAnimation > 0 && ((projectile.friendly || !projectile.hostile) || projectile.minion) && projInfo.shaderID < 0) {
				//Main.NewText("Ammo shader for projectile: " + CustomizerMod.mod.ammoShaders[projectile.owner]);
				if(projectile.type == ProjectileID.VortexBeater || CustomizerMod.mod.ammoShaders[projectile.owner].ID <= 0) {
					projInfo.shaderID = CustomizerMod.mod.heldShaders[projectile.owner].ID;
				} else {
					projInfo.shaderID = CustomizerMod.mod.ammoShaders[projectile.owner].ID;
				}
			} else if(projInfo.shaderID < 0) {
				projInfo.shaderID = 0;
			}

			newProjectiles = new List<int>();
			return true;
		}

		public override void PostAI(Projectile projectile) {
			ShadeChildren(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft) {
			newProjectiles = new List<int>();
			return base.PreKill(projectile, timeLeft);
		}

		public override void Kill(Projectile projectile, int timeLeft) {
			ShadeChildren(projectile);
		}

		public void ShadeChildren(Projectile projectile) {
			CustomizerProjInfo info = projectile.GetGlobalProjectile<CustomizerProjInfo>(mod);

			if(info.shaderID > 0) {
				foreach(int proj in newProjectiles) {
					CustomizerProjInfo childInfo = Main.projectile[proj].GetGlobalProjectile<CustomizerProjInfo>(mod);

					childInfo.shaderID = projectile.type == ProjectileID.VortexBeater ? CustomizerMod.mod.ammoShaders[projectile.owner].ID : info.shaderID;
					childInfo.parent = false;
				}

				newProjectiles = new List<int>();
			}
		}
	}
}

using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static ItemCustomizer.CustomUtils;

namespace ItemCustomizer
{
	public class CustomizerProjectile : GlobalProjectile
	{
		public static List<Projectile> newProjectiles;
		public static List<Projectile> childProjectiles;
		public static List<int> newDusts;
		public static List<int> tempDusts;

		public override void SetDefaults(Projectile projectile) {
			newProjectiles.Add(projectile);
			childProjectiles.Add(projectile);
		}

		public override bool PreAI(Projectile projectile) {
			NetExclude(NetmodeID.Server, () => {
				CustomizerProjInfo projInfo = projectile.GetGlobalProjectile<CustomizerProjInfo>();

				bool hook = Main.projHook[projectile.type];
				bool pet = (projectile.type == ProjectileID.StardustGuardian) || (Main.projPet[projectile.type] && !projectile.minion && projectile.damage == 0 && !ProjectileID.Sets.LightPet[projectile.type]);
				bool lightPet = !projectile.minion && projectile.damage == 0 && ProjectileID.Sets.LightPet[projectile.type];

				//Apply shader of the player's held item provided it meets all this criteria to make absolute certain a player fired it
				//As of 1.1.3 this now only applies to OTHER PLAYER's projectiles! Until I can get the better system working with multiplayer, that is.
				if(projInfo.parent && !(hook || pet || lightPet) && !projectile.npcProj && !projectile.trap && projectile.owner != 255 && projectile.owner != Main.myPlayer && Main.player[projectile.owner].itemAnimation > 0 && ((projectile.friendly || !projectile.hostile) || projectile.minion) && projInfo.shaderID < 0) {
					//Main.NewText("Ammo shader for projectile: " + CustomizerMod.mod.ammoShaders[projectile.owner]);
					var player = Main.player[projectile.owner];
					if(!player.HeldItem.IsAir) {
						if(player.HeldItem.useAmmo == AmmoID.None || projectile.type == ProjectileID.VortexBeater) {
							projInfo.shaderID = player.HeldItem.Customizer().shaderID.ID;
						} else {
							projInfo.shaderID = CustomizerMod.mod.ammoShaders[projectile.owner].ID;
						}
					}
				} else if(projInfo.shaderID < 0) {
					projInfo.shaderID = 0;
				}
				
				childProjectiles = new List<Projectile>();
				//newDusts = new List<int>();
			});
			return true;
		}

		public override void PostAI(Projectile projectile) {
			NetExclude(NetmodeID.Server, () => ShadeChildren(projectile));
			//ShadeDusts(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft) {
			NetExclude(NetmodeID.Server, () => childProjectiles = new List<Projectile>());
			return true;
		}

		public override void Kill(Projectile projectile, int timeLeft) {
			NetExclude(NetmodeID.Server, () => {
				ShadeChildren(projectile);
				ShadeDusts(projectile);
			});
		}

		public void ShadeChildren(Projectile projectile) {
			CustomizerProjInfo info = projectile.GetGlobalProjectile<CustomizerProjInfo>();

			if(info.shaderID > 0) {
				foreach(Projectile proj in childProjectiles) {
					CustomizerProjInfo childInfo = proj.GetGlobalProjectile<CustomizerProjInfo>();

					var player = Main.player[projectile.owner];
					childInfo.UpdateShaderID(projectile, projectile.type == ProjectileID.VortexBeater && !player.HeldItem.IsAir ? CustomizerMod.mod.ammoShaders[projectile.owner].ID : info.shaderID);
					childInfo.parent = false;
				}

				childProjectiles = new List<Projectile>();
			}
		}

		public void ShadeDusts(Projectile projectile) {
			CustomizerProjInfo info = projectile.GetGlobalProjectile<CustomizerProjInfo>();

			if(info.shaderID > 0) {
				foreach(int dust in newDusts) {
					Main.dust[dust].shader = GameShaders.Armor.GetSecondaryShader(info.shaderID, Main.player[projectile.owner]);
				}

				newDusts = new List<int>();
			}
		}

		// Attempt to skip dusts caused by cutting tiles
		public override bool? CanCutTiles(Projectile projectile) {
			NetExclude(NetmodeID.Server, () => {
				tempDusts = newDusts;
				newDusts = new List<int>();
			});
			return base.CanCutTiles(projectile);
		}
		public override void CutTiles(Projectile projectile) {
			NetExclude(NetmodeID.Server, () => {
				newDusts = tempDusts;
				tempDusts = new List<int>();
			});
		}

		// Paintball gun paints NPCs
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			NetExclude(NetmodeID.Server, () => {
				// Don't try to paint dead NPCs. It doesn't turn out very well.
				if(projectile.type == ProjectileID.PainterPaintball && damage < target.life) {
					var projInfo = projectile.GetGlobalProjectile<CustomizerProjInfo>();
					var npcInfo = target.GetGlobalNPC<CustomizerNPCInfo>();
					if(projInfo.shaderID != npcInfo.shaderID) {
						npcInfo.shaderID = projInfo.shaderID;
						CustomizerMod.mod.SendNPCShaderPacket(target, npcInfo);
					}
				}
			});
		}
	}
}

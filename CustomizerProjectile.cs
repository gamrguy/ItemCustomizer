using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerProjectile : GlobalProjectile
	{
		public static List<int> newProjectiles = new List<int>();
		public static List<int> newDusts = new List<int>();
		public static List<int> tempDusts = new List<int>();

		public override void SetDefaults(Projectile projectile) {
			newProjectiles.Add(projectile.whoAmI);
		}

		public override bool PreAI(Projectile projectile) {
			CustomizerProjInfo projInfo = projectile.GetGlobalProjectile<CustomizerProjInfo>();
			
			bool hook = Main.projHook[projectile.type];
			bool pet = (projectile.type == ProjectileID.StardustGuardian) || (Main.projPet[projectile.type] && !projectile.minion && projectile.damage == 0 && !ProjectileID.Sets.LightPet[projectile.type]);
			bool lightPet = !projectile.minion && projectile.damage == 0 && ProjectileID.Sets.LightPet[projectile.type];

			//Apply shader of the player's held item provided it meets all this criteria to make absolute certain a player fired it
			//As of 1.1.3 this now only applies to OTHER PLAYER's projectiles! Until I can get the better system working with multiplayer, that is.
			if(projInfo.parent && !(hook || pet || lightPet) && !projectile.npcProj && !projectile.trap && projectile.owner != 255 && projectile.owner != Main.myPlayer && Main.player[projectile.owner].itemAnimation > 0 && ((projectile.friendly || !projectile.hostile) || projectile.minion) && projInfo.shaderID < 0) {
				//Main.NewText("Ammo shader for projectile: " + CustomizerMod.mod.ammoShaders[projectile.owner]);
				if(Main.player[projectile.owner].HeldItem.useAmmo == AmmoID.None || projectile.type == ProjectileID.VortexBeater) {
					projInfo.shaderID = CustomizerMod.mod.heldShaders[projectile.owner].ID;
				} else {
					projInfo.shaderID = CustomizerMod.mod.ammoShaders[projectile.owner].ID;
				}
			} else if(projInfo.shaderID < 0) {
				projInfo.shaderID = 0;
			}

			newProjectiles = new List<int>();
			//newDusts = new List<int>();
			return true;
		}

		public override void PostAI(Projectile projectile) {
			ShadeChildren(projectile);
			//ShadeDusts(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft) {
			newProjectiles = new List<int>();
			return true;
		}

		public override void Kill(Projectile projectile, int timeLeft) {
			ShadeChildren(projectile);
			ShadeDusts(projectile);
		}

		public void ShadeChildren(Projectile projectile) {
			CustomizerProjInfo info = projectile.GetGlobalProjectile<CustomizerProjInfo>();

			if(info.shaderID > 0) {
				foreach(int proj in newProjectiles) {
					CustomizerProjInfo childInfo = Main.projectile[proj].GetGlobalProjectile<CustomizerProjInfo>();

					childInfo.shaderID = projectile.type == ProjectileID.VortexBeater ? CustomizerMod.mod.ammoShaders[projectile.owner].ID : info.shaderID;
					childInfo.parent = false;
				}

				newProjectiles = new List<int>();
			}
		}

		public void ShadeDusts(Projectile projectile)
		{
			CustomizerProjInfo info = projectile.GetGlobalProjectile<CustomizerProjInfo>();

			if (info.shaderID > 0)
			{
				foreach (int dust in newDusts)
				{
					Main.dust[dust].shader = GameShaders.Armor.GetSecondaryShader(info.shaderID, Main.player[projectile.owner]);
				}

				newDusts = new List<int>();
			}
		}

		// Attempt to skip dusts caused by cutting tiles
		public override bool? CanCutTiles(Projectile projectile)
		{
			tempDusts = newDusts;
			newDusts = new List<int>();
			return base.CanCutTiles(projectile);
		}
		public override void CutTiles(Projectile projectile)
		{
			newDusts = tempDusts;
			tempDusts = new List<int>();
		}

		// Paintball gun paints NPCs
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if (Main.netMode != NetmodeID.Server && projectile.type == ProjectileID.PainterPaintball)
			{
				var projInfo = projectile.GetGlobalProjectile<CustomizerProjInfo>();
				var npcInfo = target.GetGlobalNPC<CustomizerNPCInfo>();
				if (projInfo.shaderID != npcInfo.shaderID)
				{
					npcInfo.shaderID = projInfo.shaderID;
					CustomizerMod.mod.SendNPCShaderPacket(target, npcInfo);
				}
			}
		}
	}
}

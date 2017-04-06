using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerProjectile : GlobalProjectile
	{
		public List<int> newProjectiles = new List<int>();

		public override void SetDefaults(Projectile projectile)
		{
			newProjectiles.Add(projectile.whoAmI);
		}

		public override bool PreAI(Projectile projectile)
		{
			CustomizerProjInfo projInfo = projectile.GetModInfo<CustomizerProjInfo>(mod);

			bool hook = Main.projHook[projectile.type];
			bool pet = (projectile.type == ProjectileID.StardustGuardian) || (Main.projPet[projectile.type] && !projectile.minion && projectile.damage == 0 && !ProjectileID.Sets.LightPet[projectile.type]);
			bool lightPet = !projectile.minion && projectile.damage == 0 && ProjectileID.Sets.LightPet[projectile.type];

			//Apply shader of the player's held item provided it meets all this criteria to make absolute certain a player fired it
			if(projInfo.parent && !(hook || pet || lightPet) && !projectile.npcProj && projectile.owner != 255 && Main.player[projectile.owner].itemAnimation > 0 && ((projectile.friendly || !projectile.hostile) || projectile.minion) && projInfo.shaderID < 0){
				projInfo.shaderID = (mod as CustomizerMod).heldShaders[projectile.owner];
			}

			newProjectiles = new List<int>();
			return base.PreAI(projectile);
		}

		public override void PostAI(Projectile projectile)
		{
			ShadeChildren(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft)
		{
			newProjectiles = new List<int>();
			return base.PreKill(projectile, timeLeft);
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			ShadeChildren(projectile);
		}

		public void ShadeChildren(Projectile projectile){
			CustomizerProjInfo info = projectile.GetModInfo<CustomizerProjInfo>(mod);

			if(info.shaderID > 0) {
				foreach(int proj in newProjectiles) {
					CustomizerProjInfo childInfo = Main.projectile[proj].GetModInfo<CustomizerProjInfo>(mod);
					childInfo.shaderID = info.shaderID;
					childInfo.parent = false;
				}
				newProjectiles = new List<int>();
			}
		}
	}
}

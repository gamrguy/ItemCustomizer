using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderLib;
using ShaderLib.System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	class CustomizerShader : GlobalShader
	{
		public override int ItemInventoryShader(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			return item.GetGlobalItem<CustomizerItem>().shaderID.ID;
		}

		public override int ItemWorldShader(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return item.GetGlobalItem<CustomizerItem>().shaderID.ID;
		}

		public override int ProjectileShader(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			return projectile.GetGlobalProjectile<CustomizerProjInfo>().shaderID;
		}

		public override void HeldItemShader(ref int shaderID, Item item, PlayerDrawInfo drawInfo) {
			shaderID = shaderID > 0 ? shaderID : CustomizerMod.mod.heldShaders[drawInfo.drawPlayer.whoAmI].ID;
		}

		public override int NPCShader(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			return npc.GetGlobalNPC<CustomizerNPCInfo>().shaderID;
		}
	}
}

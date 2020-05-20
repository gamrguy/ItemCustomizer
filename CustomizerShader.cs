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
		
		/*public override void PlayerShader(ref PlayerShaderData data, PlayerDrawInfo drawInfo)
		{
			var player = drawInfo.drawPlayer;

			if (player.HeadDye().IsAir)
			{
				if (!player.armor[10].IsAir) drawInfo.headArmorShader = player.armor[10].GetGlobalItem<CustomizerItem>().shaderID.ID;
				else if (!player.armor[0].IsAir) drawInfo.headArmorShader = player.armor[0].GetGlobalItem<CustomizerItem>().shaderID.ID;
			}
			if (player.BodyDye().IsAir)
			{
				if (!player.armor[11].IsAir) drawInfo.bodyArmorShader = player.armor[11].GetGlobalItem<CustomizerItem>().shaderID.ID;
				else if (!player.armor[1].IsAir) drawInfo.bodyArmorShader = player.armor[1].GetGlobalItem<CustomizerItem>().shaderID.ID;
			}
			if (player.LegsDye().IsAir)
			{
				if (!player.armor[12].IsAir)
				{
					drawInfo.legArmorShader = player.armor[12].GetGlobalItem<CustomizerItem>().shaderID.ID;
					player.cLegs = player.armor[12].GetGlobalItem<CustomizerItem>().shaderID.ID;
				}
				else if (!player.armor[2].IsAir)
				{
					drawInfo.legArmorShader = player.armor[2].GetGlobalItem<CustomizerItem>().shaderID.ID;
					player.cLegs = player.armor[2].GetGlobalItem<CustomizerItem>().shaderID.ID;
				}
			}
			if (player.wearsRobe)
			{
				drawInfo.legArmorShader = drawInfo.bodyArmorShader;
			}

			int shaderId;

			void ApplyShader(ref int output, int slot, int max)
			{
				if (slot > 0 && slot < max)
				{
					output = shaderId;
				}
			}

			for (int x = 0; x < 20; x++)
			{
				int num = x % 10;

				Item accessory = player.armor[x];

				if (player.dye[num].IsAir && !accessory.IsAir && accessory.stack > 0
					&& (x / 10 >= 1 || !player.hideVisual[num] || accessory.wingSlot > 0 || accessory.type == ItemID.FlyingCarpet))
				{
					shaderId = player.armor[x].GetGlobalItem<CustomizerItem>().shaderID.ID;
					ApplyShader(ref drawInfo.handOnShader, accessory.handOnSlot, 19);
					ApplyShader(ref drawInfo.handOffShader, accessory.handOffSlot, 12);
					ApplyShader(ref drawInfo.backShader, accessory.backSlot, 11);
					ApplyShader(ref drawInfo.frontShader, accessory.frontSlot, 5);
					ApplyShader(ref drawInfo.shoeShader, accessory.shoeSlot, 18);
					ApplyShader(ref drawInfo.waistShader, accessory.waistSlot, 12);
					ApplyShader(ref drawInfo.shieldShader, accessory.shieldSlot, 6);
					ApplyShader(ref drawInfo.neckShader, accessory.neckSlot, 9);
					ApplyShader(ref drawInfo.faceShader, accessory.faceSlot, 9);
					ApplyShader(ref drawInfo.balloonShader, accessory.balloonSlot, 16);
					ApplyShader(ref drawInfo.wingShader, accessory.wingSlot, 37);

					if (accessory.type == ItemID.FlyingCarpet)
					{
						drawInfo.carpetShader = shaderId;
					}
				}
			}
		}*/
	}
}

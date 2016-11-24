/*using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomTile : ModTile
	{
		
		public override void MouseOver(int i, int j, int type)
		{
			if(type == TileID.DyeVat) {
				Main.player[Main.myPlayer].showItemIcon = true;
				Main.player[Main.myPlayer].showItemIcon2 = ItemID.DyeVat;
			}
		}

		public override void RightClick(int i, int j, int type)
		{
			if(type == TileID.DyeVat) {
				Main.playerInventory = true;
				CustomizerMod.guiOn = true;
			}
		}

		public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			//Main.tileBatch.End();
			//Main.tileBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
			//Main.tileBatch.Begin();
			GameShaders.Armor.ApplySecondary(113, Main.player[0], null);

			return true;
		}
	}
}*/
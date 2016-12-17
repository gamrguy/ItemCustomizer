using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		//public List<int> shotProjectiles = new List<int>();

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));

			DrawData data = new DrawData();
			data.origin = origin;
			data.position = position - Main.screenPosition;
			data.scale = new Vector2(scale, scale);
			data.sourceRect = frame;
			data.texture = Main.itemTexture[item.type];
			GameShaders.Armor.ApplySecondary(info.shaderID, Main.player[item.owner], data);

			return true;
		}

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
		}

		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));

			DrawData data = new DrawData();
			data.origin = item.Center;
			data.position = item.position - Main.screenPosition;
			data.scale = new Vector2(scale, scale);
			//data.sourceRect = item.;
			data.texture = Main.itemTexture[item.type];
			data.rotation = rotation;
			GameShaders.Armor.ApplySecondary(info.shaderID, Main.player[item.owner], data);

			return true;
		}

		public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
		}

		public override bool NeedsSaving(Item item)
		{
			return item.GetModInfo<CustomizerItemInfo>(mod).customData;
		}

		//New save function
		public override TagCompound Save(Item item)
		{
			//Save custom item info
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			return new TagCompound{
				{"Name", info.itemName},
				{"ShaderID", info.shaderID}
			};
		}

		//New load
		public override void Load(Item item, TagCompound tag)
		{
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			info.itemName = tag.GetString("Name");
			info.shaderID = tag.GetInt("ShaderID");

			//Set the item's name correctly
			if(info.itemName != "") item.name = info.itemName;
		}

		//Old load option, included for compatibility reasons
		public override void LoadLegacy(Item item, System.IO.BinaryReader reader)
		{
			//Load custom item info
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			info.itemName = reader.ReadString();
			info.shaderID = reader.ReadInt16();

			//Set the item's name correctly
			if(info.itemName != "") item.name = info.itemName;
		}
	
		/*
		public override bool CanUseItem(Item item, Player player)
		{
			shotProjectiles = new List<int>();
			return base.CanUseItem(item, player);
		}

		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			CustomizerPlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<CustomizerPlayer>(mod);

			if(modPlayer.heldShaders[Main.myPlayer] > 0) {
				foreach(int proj in shotProjectiles) {
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetModInfo<CustomizerProjInfo>(mod);
					shotInfo.shaderID = modPlayer.heldShaders[Main.myPlayer];
				}
				shotProjectiles = new List<int>();
			}
		}*/
	}
}


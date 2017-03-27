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
using ShaderLib.Shaders;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		//public List<int> shotProjectiles = new List<int>();

		public override bool NeedsSaving(Item item)
		{
			return item.GetModInfo<CustomizerItemInfo>(mod).customData;
		}

		//New save function
		public override TagCompound Save(Item item)
		{
			//Save custom item info
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			TagCompound data = new TagCompound{
				{"Name", info.itemName},
				{"Shader", ShaderIO.SaveShader(info.shaderID)},
			};

			return data;
		}

		//New load
		public override void Load(Item item, TagCompound tag)
		{
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

			//TODO: Replace shader loading with ShaderLib implementation
			info.itemName = tag.GetString("Name");
			info.shaderID = tag.ContainsKey("ShaderID") ? tag.GetInt("ShaderID") : ShaderIO.LoadShader(tag.GetCompound("Shader"));

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


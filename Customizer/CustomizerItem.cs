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
using ShaderLib;
using ShaderLib.Dyes;
using ShaderLib.Shaders;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		/*
		public enum ShaderType
		{
			VANILLA, MODDED, META
		}*/

		public override bool NeedsCustomSaving(Item item)
		{
			return item.GetModInfo<CustomizerItemInfo>(mod).customData;
		}

		public override void SaveCustomData(Item item, System.IO.BinaryWriter writer)
		{
			//Save custom item info
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);
			//MetaDyeInfo metaInfo = item.GetModInfo<MetaDyeInfo>(ModLoader.GetMod("ShaderLib"));

			writer.Write(info.itemName);
			ShaderIO.WriteArmorShader(info.itemID, writer);

			/*
			writer.Write(info.itemdata.Item1);
			int type = ModLoader.GetMod(info.itemdata.Item1)?.ItemType(info.itemdata.Item2) ?? 0;
			if((ModLoader.GetMod("ShaderLib") as ShaderLibMod).unLinkedItems.Contains(type)) {
				writer.Write((byte)ShaderType.META);
				writer.Write((byte)metaInfo.components.Count);
				foreach(var component in metaInfo.components) {
					writer.Write(component.Key.Item1);
					writer.Write(component.Key.Item2);
					writer.Write((byte)component.Value);
				}
				writer.Write(info.itemdata.Item2);
			} else if(type != 0) {
				writer.Write((byte)ShaderType.MODDED);
				writer.Write(info.itemdata.Item2);
			} else {
				writer.Write((byte)ShaderType.VANILLA);
				writer.Write(int.Parse(info.itemdata.Item2));
			}*/
		}

		public override void LoadCustomData(Item item, System.IO.BinaryReader reader)
		{
			try{
			//Load custom item info
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);
			/*MetaDyeInfo metaInfo = item.GetModInfo<MetaDyeInfo>(ModLoader.GetMod("ShaderLib"));
			info.itemName = reader.ReadString();

			info.itemdata = new Tuple<string, string>(reader.ReadString(), info.itemdata.Item2);
			int type = 0;
			switch((ShaderType)reader.ReadByte()) {
			case(ShaderType.META):
				int count = reader.ReadByte();
				for(int i = 0; i < count; i++) {
					var entry = new Tuple<string, string>(reader.ReadString(), reader.ReadString());
					var effect = (MetaDyeInfo.DyeEffects)reader.ReadByte();
					metaInfo.components.Add(entry, effect);
				}
				info.shaderID = MetaDyeLoader.FindPreexistingShader(metaInfo);
				info.shaderID = metaInfo.fakeItemID = GameShaders.Armor.GetShaderIdFromItemId(info.shaderID);
				break;
			case(ShaderType.MODDED):
				info.itemdata = new Tuple<string, string>(info.itemdata.Item1, reader.ReadString());
				type = ModLoader.GetMod(info.itemdata.Item1).ItemType(info.itemdata.Item2);
				info.shaderID = GameShaders.Armor.GetShaderIdFromItemId(type);
				break;
			case(ShaderType.VANILLA):
				info.itemdata = new Tuple<string, string>(info.itemdata.Item1, reader.ReadInt32().ToString());
				type = ModLoader.GetMod(info.itemdata.Item1).ItemType(info.itemdata.Item2);
				info.shaderID = GameShaders.Armor.GetShaderIdFromItemId(type);
				break;
			}*/

			info.itemName = reader.ReadString();
			info.shaderID = ShaderIO.ReadArmorShader(reader);
			
			//Set the item's name correctly
			if(info.itemName != "") item.name = info.itemName;
			} catch(Exception e) {
				//Catches loading errors due to version incompatibilities
				ErrorLogger.Log(e.ToString());
			}
		}

		/*
		public override bool UseItem(Item item, Player player)
		{
			CustomizerProjectile globalProj = (CustomizerProjectile)mod.GetGlobalProjectile("CustomizerProjectile");
			globalProj.newProjectiles = new List<int>();

			return base.UseItem(item, player);
		}*/
	}
}


using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ShaderLib.Shaders;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		//Used for saving data through reforges
		static string reforgeName = "";
		static int reforgeShader;

		//Item globals
		public string itemName = "";
		public int shaderID = 0;

		public bool customData { get { return !(itemName == "" && shaderID == 0); } }

		public override bool InstancePerEntity { get { return true; } }
		public override bool CloneNewInstances { get { return true; } }

		//public List<int> shotProjectiles = new List<int>();

		public override bool NeedsSaving(Item item)
		{
			return customData;
		}

		//Saves data
		public override TagCompound Save(Item item)
		{
			//Save custom item info
			TagCompound data = new TagCompound();
			data.Set("Name", itemName);
			data.Set("Shader", ShaderIO.SaveShader(shaderID));

			return data;
		}

		//Loads data
		public override void Load(Item item, TagCompound tag)
		{
			itemName = tag.GetString("Name");      //Loading for vanilla shaders | Loading for modded shaders
			shaderID = tag.ContainsKey("ShaderID") ? tag.GetInt("ShaderID") : ShaderIO.LoadShader(tag.GetCompound("Shader"));

			//Set the item's name correctly
			if(itemName != "") item.SetNameOverride(itemName);
		}

		//Loads data from before 0.9
		public override void LoadLegacy(Item item, System.IO.BinaryReader reader)
		{
			itemName = reader.ReadString();
			shaderID = reader.ReadInt16();

			//Set the item's name correctly
			if(itemName != "") item.SetNameOverride(itemName);
		}

		//Stops items from losing their noice data on reforge
		public override void PreReforge(Item item)
		{
			reforgeName = itemName;
			reforgeShader = shaderID;
		}

		public override void PostReforge(Item item)
		{
			itemName = reforgeName;
			shaderID = reforgeShader;
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(itemName);
			writer.Write(shaderID);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			itemName = reader.ReadString();
			shaderID = reader.ReadInt32();
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
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetGlobalItem<CustomizerProjInfo>(mod);
					shotInfo.shaderID = modPlayer.heldShaders[Main.myPlayer];
				}
				shotProjectiles = new List<int>();
			}
		}*/
	}
}

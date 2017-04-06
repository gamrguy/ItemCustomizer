using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ShaderLib.Shaders;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		//Used for saving data through reforges
		string reforgeName = "";
		int reforgeShader;
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

			TagCompound data = new TagCompound();
			data.Set("Name", info.itemName);
			data.Set("Shader", ShaderIO.SaveShader(info.shaderID));

			return data;
		}

		//New load
		public override void Load(Item item, TagCompound tag)
		{
			CustomizerItemInfo info = item.GetModInfo<CustomizerItemInfo>(mod);

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

		//Stops items from losing their noice data on reforge
		public override void PreReforge(Item item)
		{
			var info = item.GetModInfo<CustomizerItemInfo>(mod);
			reforgeName = info.itemName;
			reforgeShader = info.shaderID;
		}

		public override void PostReforge(Item item)
		{
			var info = item.GetModInfo<CustomizerItemInfo>(mod);
			info.itemName = reforgeName;
			info.shaderID = reforgeShader;
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

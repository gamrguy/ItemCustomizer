using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ShaderLib;
using Terraria.Graphics.Shaders;
using Steamworks;

namespace ItemCustomizer
{
	public class CustomizerItem : GlobalItem
	{
		//Used for saving data through reforges
		static string reforgeName = "";
		static ShaderID reforgeShader;

		//Item globals
		public string itemName = "";
		public ShaderID shaderID = new ShaderID(-1);

		public bool CustomData => !(itemName == "" && shaderID.ID == -1);

		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;

		public override void UseItemHitbox(Item item, Player player, ref Microsoft.Xna.Framework.Rectangle hitbox, ref bool noHitbox)
		{
			CustomizerProjectile.newDusts = new List<int>();
		}

		public override void MeleeEffects(Item item, Player player, Microsoft.Xna.Framework.Rectangle hitbox)
		{
			if (shaderID.ID > 0)
			{
				foreach (int dust in CustomizerProjectile.newDusts)
				{
					Main.dust[dust].shader = GameShaders.Armor.GetSecondaryShader(shaderID.ID, Main.player[item.owner]);
				}

				CustomizerProjectile.newDusts = new List<int>();
			}
		}

		public override bool NeedsSaving(Item item)
		{
			return CustomData;
		}

		//Saves data
		public override TagCompound Save(Item item)
		{
			//Save custom item info
			TagCompound data = new TagCompound();
			data.Set("Name", itemName);
			data.Set("Shader", ShaderID.Save(shaderID));

			return data;
		}

		//Loads data
		public override void Load(Item item, TagCompound tag)
		{
			itemName = tag.GetString("Name");
			shaderID = ShaderID.Load(tag.GetCompound("Shader"));

			//Set the item's name correctly
			if(itemName != "") item.SetNameOverride(itemName);
		}

		//Stops items from losing their noice data on reforge
		public override bool NewPreReforge(Item item)
		{
			reforgeName = itemName;
			reforgeShader = shaderID;
			return base.NewPreReforge(item);
		}

		public override void PostReforge(Item item)
		{
			itemName = reforgeName;
			shaderID = reforgeShader;
		}

		//Stops items from losing their noice data when dropped in multiplayer
		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(itemName);
			ShaderID.Write(writer, shaderID);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			itemName = reader.ReadString();
			shaderID = ShaderID.Read(reader);
		}
	}
}

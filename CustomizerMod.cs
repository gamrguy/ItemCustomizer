﻿using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerraUI;
using TerraUI.Utilities;
using ShaderLib.Shaders;

namespace ItemCustomizer
{
	public class CustomizerMod : Mod
	{
		//This silly string is used to make sure the sent packet isn't corrupted or anything stupid.
		public const string pakCheckString = "Ach, Hans Run! It's the...";
		public int[] heldShaders = new int[Main.maxPlayers];
		public bool guiOn;

		public CustomizerMod()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			UIUtils.Mod = this;
			UIUtils.Subdirectory = "TerraUI/";

			ItemShader.preDrawInv.Add(delegate(int shaderID, Item item) {
				int shader = item.GetModInfo<CustomizerItemInfo>(TerraUI.Utilities.UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});
			ItemShader.preDrawWorld.Add(delegate(int shaderID, Item item) {
				int shader = item.GetModInfo<CustomizerItemInfo>(TerraUI.Utilities.UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});

			ProjectileShader.hooks.Add(delegate(int shaderID, Projectile projectile) {
				int shader = projectile.GetModInfo<CustomizerProjInfo>(TerraUI.Utilities.UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});
		}

		public override void AddRecipeGroups()
		{
			//Creates a recipe group for the four Strange Plant items.
			Item item = new Item();
			item.SetDefaults(ItemID.StrangePlant1);
			RecipeGroup group = new RecipeGroup(() => Lang.misc[37] + " " + item.name, new int[]
				{
					ItemID.StrangePlant1,
					ItemID.StrangePlant2,
					ItemID.StrangePlant3,
					ItemID.StrangePlant4
				});
			RecipeGroup.RegisterGroup("ItemCustomizer:AnyStrangePlant", group);
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			UIPlayer player = Main.player[Main.myPlayer].GetModPlayer<UIPlayer>(this);

			if(!Main.playerInventory)
				guiOn = false;
			
			if(guiOn) {
				player.DrawUI(spriteBatch);
			}
			if(Main.autoPause && guiOn) {
				Main.player[Main.myPlayer].itemTime = 0;
				Main.player[Main.myPlayer].itemAnimation = 0;
			}
		}

		public override void PostUpdateInput()
		{
			if(Main.autoPause && guiOn) {
				Main.player[Main.myPlayer].GetModPlayer<UIPlayer>(this).UpdateUI();
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			if(reader.ReadString() == pakCheckString) {
				int heldShader = reader.ReadInt32();
				
				if(Main.netMode == 2) {
					ModPacket pak = GetPacket();
					pak.Write(pakCheckString);
					pak.Write(heldShader);
					pak.Write(whoAmI);
					pak.Send(ignoreClient: whoAmI);
				} else {
					heldShaders[reader.ReadInt32()] = heldShader;
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.UI;
using ShaderLib;

namespace ItemCustomizer
{
	public class CustomizerMod : Mod
	{
		//This silly string is used to make sure the sent packet isn't corrupted or anything stupid.
		public const string pakCheckString = "Ach, Hans Run! It's the...";
		public int[] heldShaders = new int[Main.maxPlayers];

		public static CustomizerMod mod;
		public CustomizerUI customizerUI;
		public UserInterface customizerInterface;

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
			if(Main.netMode != NetmodeID.Server) {
				customizerUI = new CustomizerUI();
				customizerUI.Activate();
				customizerInterface = new UserInterface();
				customizerInterface.SetState(customizerUI);
			}

			mod = this;

			//Adding ShaderLib hooks
			ShaderLibMod.AddItemShaderInventoryHook((shaderID, item) => {
				int shader = item.GetGlobalItem<CustomizerItem>(mod).shaderID;
				return shader > 0 ? shader : shaderID;
			});
			ShaderLibMod.AddItemShaderWorldHook((shaderID, item) => {
				int shader = item.GetGlobalItem<CustomizerItem>(mod).shaderID;
				return shader > 0 ? shader : shaderID;
			});
			ShaderLibMod.AddProjectileShaderHook((shaderID, projectile) => {
				int shader = projectile.GetGlobalProjectile<CustomizerProjInfo>(mod).shaderID;
				return shader > 0 ? shader : shaderID;
			});
			ShaderLibMod.AddHeldItemShaderHook((shaderID, heldItem, player) => {
				int shader = heldShaders[player.whoAmI];
				return shader > 0 ? shader : shaderID;
			});
		}

		public override void AddRecipeGroups()
		{
			//Creates a recipe group for the four Strange Plant items.
			RecipeGroup group = new RecipeGroup(() => Language.GetText("Any") + " " + Lang.GetItemName(ItemID.StrangePlant1), new int[]
				{
					ItemID.StrangePlant1,
					ItemID.StrangePlant2,
					ItemID.StrangePlant3,
					ItemID.StrangePlant4
				});
			RecipeGroup.RegisterGroup("ItemCustomizer:AnyStrangePlant", group);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if(mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"ItemCustomizer:CustomizerUI",
					delegate {
						if(CustomizerUI.visible) {
							customizerInterface.Update(Main._drawInterfaceGameTime);
							customizerUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			CustomizerUI.visible &= Main.playerInventory;

			//                          Don't cause the stupid multiplayer rapidfire bug :P
			if(Main.autoPause && CustomizerUI.visible && Main.netMode != NetmodeID.MultiplayerClient) {
				Main.player[Main.myPlayer].itemTime = 0;
				Main.player[Main.myPlayer].itemAnimation = 0;
			}
		}

		/*public override void PostUpdateInput()
		{
			if(Main.autoPause && guiOn) {
				//Main.player[Main.myPlayer].GetModPlayer<UIPlayer>(this).UpdateUI();
			}
		}*/

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			if(reader.ReadString() == pakCheckString) {
				int heldShader = reader.ReadInt32();

				if(Main.netMode == NetmodeID.Server) {
					heldShaders[whoAmI] = heldShader; //Server now collects heldShader data too for consistency
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

		//Weak referencing functions, have fun flashkirby99 :D
		public override object Call(params object[] args)
		{
			var badStuffException = new Exception("Something bad happened. Perhaps you're missing an argument?");
			var notAnItemException = new ArgumentException("Incorrect syntax. Try sending in an Item type next time.");
			var notAProjException = new ArgumentException("Incorrect syntax. Try sending in a Projectile type next time.");
			var invalidCommandException = new Exception("Incorrect command. Are you spelling the command correctly?");

			try {
				switch((string)args[0]) {
				case "GetItemShader":
					if((args[1] as Item) != null) {
						return (args[1] as Item).GetGlobalItem<CustomizerItem>(this);
					}
					throw notAnItemException;
				case "GetProjShader":
					if((args[1] as Projectile) != null) {
						return (args[1] as Projectile).GetGlobalProjectile<CustomizerProjInfo>(this).shaderID;
					}
					throw notAProjException;
				case "SetItemShader":
					if((args[1] as Item) != null) {
						(args[1] as Item).GetGlobalItem<CustomizerItem>(this).shaderID = (int)args[2];
						return true;
					}
					throw notAnItemException;
				case "SetProjShader":
					if((args[1] as Projectile) != null) {
						(args[1] as Projectile).GetGlobalProjectile<CustomizerProjInfo>(this).shaderID = (int)args[2];
						return true;
					}
					throw notAProjException;
				case "GetItemName":
					if((args[1] as Item) != null) {
						return (args[1] as Item).GetGlobalItem<CustomizerItem>(this).itemName;
					}
					throw notAnItemException;
				case "SetItemName":
					if((args[1] as Item) != null) {
						(args[1] as Item).GetGlobalItem<CustomizerItem>(this).itemName = (string)args[2];
						return true;
					}
					throw notAnItemException;
				}
			} catch {
				throw badStuffException;
			}

			throw invalidCommandException;
		}
	}
}

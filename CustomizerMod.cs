using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerraUI;
using TerraUI.Utilities;
using ShaderLib;
using ShaderLib.Shaders;
using ShaderLib.Dyes;

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

			//Adding ShaderLib hooks
			ShaderLibMod.AddItemShaderInventoryHook((shaderID, item) => {
				int shader = item.GetModInfo<CustomizerItemInfo>(UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});
			ShaderLibMod.AddItemShaderWorldHook((shaderID, item) => {
				int shader = item.GetModInfo<CustomizerItemInfo>(UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});
			ShaderLibMod.AddProjectileShaderHook((shaderID, projectile) => {
				int shader = projectile.GetModInfo<CustomizerProjInfo>(UIUtils.Mod).shaderID;
				return shader != 0 ? shader : shaderID;
			});
			ShaderLibMod.AddHeldItemShaderHook((shaderID, heldItem, player) => {
				int shader = heldShaders[player.whoAmI];
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
			UIPlayer player = Main.LocalPlayer.GetModPlayer<UIPlayer>(this);

			if(!Main.playerInventory)
				guiOn = false;
			
			if(guiOn) {
				player.DrawUI(spriteBatch);
			}

			//                          Don't cause the stupid multiplayer rapidfire bug :P
			if(Main.autoPause && guiOn && Main.netMode != NetmodeID.MultiplayerClient) {
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
						return (args[1] as Item).GetModInfo<CustomizerItemInfo>(this);
					}
					return notAnItemException;
				case "GetProjShader":
					if((args[1] as Projectile) != null) {
						return (args[1] as Projectile).GetModInfo<CustomizerProjInfo>(this).shaderID;
					}
					return notAProjException;
				case "SetItemShader":
					if((args[1] as Item) != null) {
						(args[1] as Item).GetModInfo<CustomizerItemInfo>(this).shaderID = (int)args[2];
						return true;
					}
					return notAnItemException;
				case "SetProjShader":
					if((args[1] as Projectile) != null) {
						(args[1] as Projectile).GetModInfo<CustomizerProjInfo>(this).shaderID = (int)args[2];
						return true;
					}
					return notAProjException;
				}
			} catch {
				return badStuffException;
			}

			return invalidCommandException;
		}
	}
}
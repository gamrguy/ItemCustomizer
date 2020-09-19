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
using Terraria.Graphics.Shaders;

namespace ItemCustomizer
{
	public class CustomizerMod : Mod
	{
		public ShaderID[] ammoShaders = new ShaderID[Main.maxPlayers];

		public static CustomizerMod mod;
		public CustomizerUI customizerUI;
		public UserInterface customizerInterface;

		public override void Load() {
			if(Main.netMode != NetmodeID.Server) {
				customizerUI = new CustomizerUI();
				customizerUI.Activate();
				customizerInterface = new UserInterface();
				customizerInterface.SetState(customizerUI);
			}

			//Here, have some absolute black magic. It's fun being a wizard! Or not.
			//Record created dusts for shading later
			On.Terraria.Dust.NewDust += Dust_Capture;

			//Capture dusts from projectile updates
			On.Terraria.Projectile.Update += Proj_Update;

			//Attempt to skip NPC hit/death dusts
			On.Terraria.NPC.VanillaHitEffect += NPC_SkipHit;

			ShaderLoader.RegisterMod(this);

			mod = this;
			CustomizerProjectile.newProjectiles = new List<Projectile>();
			CustomizerProjectile.childProjectiles = new List<Projectile>();
			CustomizerProjectile.newDusts = new List<int>();
			CustomizerProjectile.tempDusts = new List<int>();
		}

		private int Dust_Capture(On.Terraria.Dust.orig_NewDust orig, Microsoft.Xna.Framework.Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Microsoft.Xna.Framework.Color newColor, float Scale) {
			int idx = orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
			CustomizerProjectile.newDusts.Add(idx);
			return idx;
		}

		private void Proj_Update(On.Terraria.Projectile.orig_Update orig, Projectile self, int i) {
			CustomizerProjectile.newDusts = new List<int>();

			orig(self, i);

			if(self.active) {
				CustomizerProjInfo info = self.GetGlobalProjectile<CustomizerProjInfo>();

				if(info.shaderID > 0) {
					foreach(int dust in CustomizerProjectile.newDusts) {
						Main.dust[dust].shader = GameShaders.Armor.GetSecondaryShader(info.shaderID, Main.player[self.owner]);
					}
				}
			}
			CustomizerProjectile.newDusts = new List<int>();
		}

		private void NPC_SkipHit(On.Terraria.NPC.orig_VanillaHitEffect orig, NPC self, int hitDirection, double dmg) {
			CustomizerProjectile.tempDusts = CustomizerProjectile.newDusts;
			CustomizerProjectile.newDusts = new List<int>();
			orig(self, hitDirection, dmg);
			CustomizerProjectile.newDusts = CustomizerProjectile.tempDusts;
			CustomizerProjectile.tempDusts = new List<int>();
		}

		public override void Unload() {
			On.Terraria.Dust.NewDust -= Dust_Capture;
			On.Terraria.Projectile.Update -= Proj_Update;
			On.Terraria.NPC.VanillaHitEffect -= NPC_SkipHit;

			mod = null;
			CustomizerProjectile.newProjectiles = null;
			CustomizerProjectile.childProjectiles = null;
			CustomizerProjectile.newDusts = null;
			CustomizerProjectile.tempDusts = null;
		}

		public override void AddRecipeGroups() {
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

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
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

		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			CustomizerUI.visible &= Main.playerInventory;

			//                          Don't cause the stupid multiplayer rapidfire bug :P
			if(Main.autoPause && CustomizerUI.visible && Main.netMode != NetmodeID.MultiplayerClient) {
				Main.player[Main.myPlayer].itemTime = 0;
				Main.player[Main.myPlayer].itemAnimation = 0;
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			var type = (PacketType)reader.ReadByte();

			if(Main.netMode == NetmodeID.Server) {
				ModPacket pak = GetPacket();
				pak.Write((byte)type);
				ShaderID.Write(pak, ShaderID.Read(reader));
				if(type == PacketType.NPC) pak.Write(reader.ReadInt32());
				else if(type == PacketType.PROJ) {
					pak.Write(reader.ReadInt32());
					pak.Write(reader.ReadByte());
                } else pak.Write(whoAmI);
				pak.Send(ignoreClient: whoAmI);
			} else {
				ShaderID shader = ShaderID.Read(reader);
				switch(type) {
					case PacketType.AMMO:
						ammoShaders[reader.ReadInt32()] = shader;
						break;
					case PacketType.NPC:
						Main.npc[reader.ReadInt32()].Customizer().shaderID = shader.ID;
						break;
					case PacketType.PROJ:
						// read in reverse order because why ???
						// if this works, what the fuck
						int identity = reader.ReadInt32();
						byte owner = reader.ReadByte();
						//int idx = Projectile.GetByUUID(owner, identity);
						int num84 = 1000;
						for(int num85 = 0; num85 < 1000; num85++) {
							if(Main.projectile[num85].owner == owner && Main.projectile[num85].identity == identity && Main.projectile[num85].active) {
								num84 = num85;
								break;
							}
						}

						if(num84 == 1000) {
							for(int num86 = 0; num86 < 1000; num86++) {
								if(!Main.projectile[num86].active) {
									num84 = num86;
									break;
								}
							}
						}
						Main.NewText("Received projectile packet: " + owner + " " + identity + " " + num84 + " " + shader.ID);
						if(num84 != -1 && num84 != Main.maxProjectiles) {
							var proj = Main.projectile[num84];
							//if(proj.Customizer() is CustomizerProjInfo customizer)
							//customizer.shaderID = shader.ID;
							proj.Customizer().shaderID = shader.ID;
						} else {
							Main.NewText("Invalid projectile identity");
						}
						break;
				}
			}
		}

		//Weak referencing functions, have fun flashkirby99 :D
		//...he just used reflection instead. figures, lmao
		public override object Call(params object[] args) {
			var badStuffException = new Exception("Something bad happened. Perhaps you're missing an argument?");
			var notAnItemException = new ArgumentException("Incorrect syntax. Try sending in an Item type next time.");
			var notAProjException = new ArgumentException("Incorrect syntax. Try sending in a Projectile type next time.");
			var invalidCommandException = new Exception("Incorrect command. Are you spelling the command correctly?");

			try {
				switch((string)args[0]) {
					case "GetItemShader":
						if((args[1] as Item) != null) {
							return (args[1] as Item).GetGlobalItem<CustomizerItem>();
						}
						throw notAnItemException;
					case "GetProjShader":
						if((args[1] as Projectile) != null) {
							return (args[1] as Projectile).GetGlobalProjectile<CustomizerProjInfo>().shaderID;
						}
						throw notAProjException;
					case "SetItemShader":
						if((args[1] as Item) != null) {
							(args[1] as Item).GetGlobalItem<CustomizerItem>().shaderID = new ShaderID((int)args[2]);
							return true;
						}
						throw notAnItemException;
					case "SetProjShader":
						if((args[1] as Projectile) != null) {
							(args[1] as Projectile).GetGlobalProjectile<CustomizerProjInfo>().shaderID = (int)args[2];
							return true;
						}
						throw notAProjException;
					case "GetItemName":
						if((args[1] as Item) != null) {
							return (args[1] as Item).GetGlobalItem<CustomizerItem>().itemName;
						}
						throw notAnItemException;
					case "SetItemName":
						if((args[1] as Item) != null) {
							(args[1] as Item).GetGlobalItem<CustomizerItem>().itemName = (string)args[2];
							return true;
						}
						throw notAnItemException;
				}
			} catch {
				throw badStuffException;
			}

			throw invalidCommandException;
		}

		public void SendAmmoShaderPacket() {
			if(Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket pak = GetPacket();
				pak.Write((byte)PacketType.AMMO);
				ShaderID.Write(pak, ammoShaders[Main.myPlayer]);
				pak.Send();
			}
		}

		public void SendNPCShaderPacket(NPC npc, CustomizerNPCInfo npcInfo) {
			if(Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket pak = GetPacket();
				pak.Write((byte)PacketType.NPC);
				ShaderID.Write(pak, new ShaderID(npcInfo.shaderID));
				pak.Write(npc.whoAmI);
				pak.Send();
			}
		}

		public void SendProjShaderPacket(Projectile projectile, CustomizerProjInfo projInfo) {
			if(Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket pak = GetPacket();
				pak.Write((byte)PacketType.PROJ);
				ShaderID.Write(pak, new ShaderID(projInfo.shaderID));
				pak.Write(projectile.owner);
				pak.Write(projectile.identity);
				pak.Send();
            }
        }

		public enum PacketType
		{
			AMMO,
			NPC,
			PROJ
		}
	}
}

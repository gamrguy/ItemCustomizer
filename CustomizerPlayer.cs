using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ShaderLib;
using Terraria.ModLoader.IO;
using ShaderLib.System;
using Terraria.Graphics.Shaders;

namespace ItemCustomizer
{
	public class CustomizerPlayer : ModPlayer
	{
		//public int ammoShader = 0;

		public override void PreUpdate()
		{
			if(Main.myPlayer != player.whoAmI) {
				return;
			}

			Item held;
			if(Main.mouseItem.active && !Main.mouseItem.IsAir && Main.mouseItem.type != ItemID.None) {
				held = Main.mouseItem;
			} else {
				held = player.inventory[player.selectedItem];
			}
			if(!held.active || held.IsAir) {
				(mod as CustomizerMod).heldShaders[player.whoAmI] = new ShaderID(-1);
			} else {
				CustomizerItem info = held.GetGlobalItem<CustomizerItem>();
				(mod as CustomizerMod).heldShaders[player.whoAmI] = info.shaderID;
			}

			CustomizerMod.mod.SendHeldShaderPacket();
		}

		public override bool PreItemCheck()
		{
			CustomizerMod.mod.ammoShaders[Main.myPlayer] = new ShaderID(-1);
			CustomizerProjectile.newProjectiles = new List<int>();
			return true;
		}

		public override void PostItemCheck()
		{
			if(CustomizerMod.mod.heldShaders[player.whoAmI].ID > 0 || CustomizerMod.mod.ammoShaders[player.whoAmI].ID > 0) {
				foreach(int proj in CustomizerProjectile.newProjectiles) {
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetGlobalProjectile<CustomizerProjInfo>();

					//Yes, there's a specific thing for the Vortex Beater. It's the only ammo-using weapon I know that displays as a projectile.
					if(player.HeldItem.useAmmo == AmmoID.None || Main.projectile[proj].type == ProjectileID.VortexBeater) {
						shotInfo.shaderID = CustomizerMod.mod.heldShaders[player.whoAmI].ID;
					} else {
						shotInfo.shaderID = CustomizerMod.mod.ammoShaders[player.whoAmI].ID;
					}
				}

				CustomizerProjectile.newProjectiles = new List<int>();
			}
		}

		public override TagCompound Save() {
			CustomizerMod realMod = mod as CustomizerMod;
			return new TagCompound{
				{"ItemSlot", ItemIO.Save(realMod.customizerUI.itemSlot.item)},
				{"DyeSlot", ItemIO.Save(realMod.customizerUI.dyeSlot.item)}
			};
		}

		public override void Load(TagCompound tag) {
			CustomizerMod realMod = mod as CustomizerMod;
			realMod.customizerUI.itemSlot.item = ItemIO.Load(tag.GetCompound("ItemSlot"));
			realMod.customizerUI.dyeSlot.item = ItemIO.Load(tag.GetCompound("DyeSlot"));
		}

		public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			if (player.HeadDye().IsAir)
			{
				if (!player.armor[10].IsAir) drawInfo.headArmorShader = player.armor[10].GetGlobalItem<CustomizerItem>().shaderID.ID;
				else if (!player.armor[0].IsAir) drawInfo.headArmorShader = player.armor[0].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (drawInfo.headArmorShader < 0) drawInfo.headArmorShader = 0;
			}
			if (player.BodyDye().IsAir)
			{
				if (!player.armor[11].IsAir) drawInfo.bodyArmorShader = player.armor[11].GetGlobalItem<CustomizerItem>().shaderID.ID;
				else if (!player.armor[1].IsAir) drawInfo.bodyArmorShader = player.armor[1].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (drawInfo.bodyArmorShader < 0) drawInfo.bodyArmorShader = 0;
			}
			if (player.LegsDye().IsAir)
			{
				if (!player.armor[12].IsAir)
				{
					drawInfo.legArmorShader = player.armor[12].GetGlobalItem<CustomizerItem>().shaderID.ID;
					player.cLegs = player.armor[12].GetGlobalItem<CustomizerItem>().shaderID.ID;
				}
				else if (!player.armor[2].IsAir)
				{
					drawInfo.legArmorShader = player.armor[2].GetGlobalItem<CustomizerItem>().shaderID.ID;
					player.cLegs = player.armor[2].GetGlobalItem<CustomizerItem>().shaderID.ID;
				}
				if (drawInfo.legArmorShader < 0) drawInfo.legArmorShader = 0;
				if (player.cLegs < 0) player.cLegs = 0;
			}
			if (player.wearsRobe)
			{
				drawInfo.legArmorShader = drawInfo.bodyArmorShader;
			}

			int shaderId;

			void ApplyShader(ref int output, int slot, int max)
			{
				if (slot > 0 && slot < max)
				{
					output = shaderId >= 0 ? shaderId : 0;
				}
			}

			for (int x = 0; x < 20; x++)
			{
				int num = x % 10;

				Item accessory = player.armor[x];

				if (player.dye[num].IsAir && !accessory.IsAir && accessory.stack > 0
					&& (x / 10 >= 1 || !player.hideVisual[num] || accessory.wingSlot > 0 || accessory.type == ItemID.FlyingCarpet))
				{
					shaderId = player.armor[x].GetGlobalItem<CustomizerItem>().shaderID.ID;
					ApplyShader(ref drawInfo.handOnShader, accessory.handOnSlot, 19);
					ApplyShader(ref drawInfo.handOffShader, accessory.handOffSlot, 12);
					ApplyShader(ref drawInfo.backShader, accessory.backSlot, 11);
					ApplyShader(ref drawInfo.frontShader, accessory.frontSlot, 5);
					ApplyShader(ref drawInfo.shoeShader, accessory.shoeSlot, 18);
					ApplyShader(ref drawInfo.waistShader, accessory.waistSlot, 12);
					ApplyShader(ref drawInfo.shieldShader, accessory.shieldSlot, 6);
					ApplyShader(ref drawInfo.neckShader, accessory.neckSlot, 9);
					ApplyShader(ref drawInfo.faceShader, accessory.faceSlot, 9);
					ApplyShader(ref drawInfo.balloonShader, accessory.balloonSlot, 16);
					ApplyShader(ref drawInfo.wingShader, accessory.wingSlot, 37);

					if (accessory.type == ItemID.FlyingCarpet)
					{
						drawInfo.carpetShader = shaderId;
					}
				}
			}
		}

		public override void PreUpdateBuffs()
		{
			if (player.PetDye().IsAir && !player.miscEquips[0].IsAir)
			{
				player.cPet = player.miscEquips[0].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (player.cPet < 0) player.cPet = 0;
			}
			if (player.LightDye().IsAir && !player.miscEquips[1].IsAir)
			{
				player.cLight = player.miscEquips[1].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (player.cLight < 0) player.cLight = 0;
			}
			if (player.MinecartDye().IsAir && !player.miscEquips[2].IsAir)
			{
				player.cMinecart = player.miscEquips[2].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (player.cMinecart < 0) player.cMinecart = 0;
			}
			if (player.MountDye().IsAir && !player.miscEquips[3].IsAir)
			{
				player.cMount = player.miscEquips[3].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (player.cMount < 0) player.cMount = 0;
			}
			if (player.GrappleDye().IsAir && !player.miscEquips[4].IsAir)
			{
				player.cGrapple = player.miscEquips[4].GetGlobalItem<CustomizerItem>().shaderID.ID;
				if (player.cGrapple < 0) player.cGrapple = 0;
			}
			player.cYorai = player.cPet;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace ItemCustomizer
{
	public class CustomizerPlayer : ModPlayer
	{
		//public List<int> shotProjectiles = new List<int>();

		public override void PreUpdate()
		{
			if(Main.myPlayer != player.whoAmI) {
				return;
			}

			Item held;
			if(Main.mouseItem.type > 0) {
				held = Main.mouseItem;
			} else {
				held = player.inventory[player.selectedItem];
			}
			if(!held.active) {
				(mod as CustomizerMod).heldShaders[Main.myPlayer] = 0;
			} else {
				CustomizerItemInfo info = held.GetModInfo<CustomizerItemInfo>(mod);
				(mod as CustomizerMod).heldShaders[Main.myPlayer] = info.shaderID;
			}

			if(Main.netMode == 1) {
				ModPacket pak = mod.GetPacket();
				pak.Write(CustomizerMod.pakCheckString);
				pak.Write((mod as CustomizerMod).heldShaders[Main.myPlayer]);
				pak.Send();
			}
		}

		public PlayerLayer weaponDye = new PlayerLayer("ItemCustomizer", "WeaponDye", delegate(PlayerDrawInfo drawInfo)
			{
				Player player = drawInfo.drawPlayer;
				Item heldItem = player.inventory[player.selectedItem];

				//Don't cause crashes :-)
				if(heldItem == null || !heldItem.active) return;
				CustomizerItemInfo info = heldItem.GetModInfo<CustomizerItemInfo>(TerraUI.Utilities.UIUtils.Mod);
				int index = Main.playerDrawData.Count-1;

				//Don't bother with items that aren't showing (e.g. Arkhalis)
				if(heldItem.noUseGraphic) return;

				//Prevents throwing a shader on torch flames (or any other flame texture)
				if(heldItem.flame){
					index-=1;
				}

				//Only use shader when player is using/holding an item
				if(player.itemAnimation > 0 && !player.pulley && info.shaderID > 0){
					DrawData data = Main.playerDrawData[index];
					data.shader = info.shaderID;
					Main.playerDrawData[index] = data;
				}
			});

		public override void ModifyDrawLayers(List<PlayerLayer> layers)
		{
			if(!Main.gameMenu) {
				layers.Insert(layers.IndexOf(PlayerLayer.HeldItem) + 1, weaponDye);
			}
		}

		public override bool PreItemCheck()
		{
			//CustomizerProjectile globalProj = (CustomizerProjectile)mod.GetGlobalProjectile("CustomizerProjectile");
			//globalProj.newProjectiles = new List<int>();
			CustomizerProjectile.newProjectiles.Clear();
			return base.PreItemCheck();
		}

		public override void PostItemCheck()
		{
			//CustomizerProjectile globalProj = (CustomizerProjectile)mod.GetGlobalProjectile("CustomizerProjectile");
			if((mod as CustomizerMod).heldShaders[player.whoAmI] > 0) {
				foreach(int proj in CustomizerProjectile.newProjectiles /*globalProj.newProjectiles*/) {
					CustomizerProjInfo shotInfo = Main.projectile[proj].GetModInfo<CustomizerProjInfo>(mod);
					shotInfo.shaderID = (mod as CustomizerMod).heldShaders[player.whoAmI];
					//Main.NewText("Applying shader " + shotInfo.shaderID + " to projectile " + proj);
				}
				CustomizerProjectile.newProjectiles.Clear();
				//globalProj.newProjectiles = new List<int>();
			}
		}
	}
}


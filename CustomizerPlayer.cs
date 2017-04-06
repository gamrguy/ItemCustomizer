using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ItemCustomizer
{
	public class CustomizerPlayer : ModPlayer
	{
		public override void PreUpdate()
		{
			if(Main.myPlayer != player.whoAmI) {
				return;
			}

			Item held;
			if(Main.mouseItem.active && !Main.mouseItem.IsAir && Main.mouseItem.type > 0) {
				held = Main.mouseItem;
			} else {
				held = player.inventory[player.selectedItem];
			}
			if(!held.active || held.IsAir) {
				(mod as CustomizerMod).heldShaders[player.whoAmI] = 0;
			} else {
				CustomizerItemInfo info = held.GetModInfo<CustomizerItemInfo>(mod);
				(mod as CustomizerMod).heldShaders[player.whoAmI] = info.shaderID;
			}

			if(Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket pak = mod.GetPacket();
				pak.Write(CustomizerMod.pakCheckString);
				pak.Write((mod as CustomizerMod).heldShaders[Main.myPlayer]);
				pak.Send();
			}
		}

		/*public PlayerLayer weaponDye = new PlayerLayer("ItemCustomizer", "WeaponDye", delegate (PlayerDrawInfo drawInfo) {
			Player player = drawInfo.drawPlayer;
			Item heldItem = player.inventory[player.selectedItem];

			//Don't cause crashes :-)
			if(heldItem == null || !heldItem.active || heldItem.IsAir) return;
			CustomizerItemInfo info = heldItem.GetModInfo<CustomizerItemInfo>(TerraUI.Utilities.UIUtils.Mod);
			int index = Main.playerDrawData.Count - 1;

			//Don't bother with items that aren't showing (e.g. Arkhalis)
			if(heldItem.noUseGraphic) return;

			//Prevents throwing a shader on torch flames (or any other flame texture)
			if(heldItem.flame) {
				index -= 1;
			}

			//Only use shader when player is using/holding an item
			if(player.itemAnimation > 0 && !player.pulley && info.shaderID > 0) {
				Main.NewText("Applying shader " + info.shaderID + " for player " + player.name);
				DrawData data = Main.playerDrawData[index];
				data.shader = info.shaderID;
				Main.playerDrawData[index] = data;
			}
		});

		public override void ModifyDrawLayers (List<PlayerLayer> layers)
		{
			if (!Main.gameMenu) {
				layers.Insert (layers.IndexOf (PlayerLayer.HeldItem) + 1, weaponDye);
			}
		}*/
	}
}

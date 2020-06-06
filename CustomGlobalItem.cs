using ShaderLib;
using Terraria;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomGlobalItem : GlobalItem
	{
		public override bool CanUseItem(Item item, Player player) {
			if(player.whoAmI == Main.myPlayer) {
				CustomizerMod.mod.ammoShaders[Main.myPlayer] = new ShaderID(-1);
				CustomizerMod.mod.SendAmmoShaderPacket();
			}

			return true;
		}

		public override bool ConsumeAmmo(Item item, Player player) {
			if(player.whoAmI == Main.myPlayer) {
				CustomizerItem info = item.Customizer();

				if(item.owner <= Main.maxPlayers) {
					CustomizerMod.mod.ammoShaders[item.owner] = info.shaderID;
				}
				//Main.NewText("Ammo shader: " + info.shaderID);
				CustomizerMod.mod.SendAmmoShaderPacket();
			}

			return true;
		}
	}
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ItemCustomizer
{
	public class CustomItem : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.IronAnvil;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Item Customizer");
			Tooltip.SetDefault("Use to open the customizer GUI");
		}

		public override void SetDefaults() {
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useAnimation = 2;
			item.useTime = 2;
			item.rare = ItemRarityID.Blue;
		}

		public override bool UseItem(Player player) {
			if(Main.myPlayer == player.whoAmI) {
				CustomizerUI.visible = true;
				Main.playerInventory = true;
			}
			player.itemAnimation = 0;
			player.itemTime = 0;

			Main.PlaySound(SoundID.MenuOpen);
			return true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipeGroup("IronBar", 5);
			recipe.AddRecipeGroup("ItemCustomizer:AnyStrangePlant", 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

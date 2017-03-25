using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics.Shaders;

namespace ItemCustomizer
{
	public class CustomItem : ModItem
	{
		public override bool Autoload (ref string name, ref string texture, System.Collections.Generic.IList<EquipType> equips)
		{
			texture = "Terraria/Item_" + ItemID.IronAnvil; //use anvil texture
			return true;
		}

		public override void SetDefaults ()
		{
			item.name = "Item Customizer";
			item.useStyle = 1;
			item.useAnimation = 2;
			item.useTime = 2;
			item.rare = 1;
			item.toolTip = "Use to open the Customization GUI";
		}

		public override bool UseItem(Player player)
		{
			if(Main.myPlayer == player.whoAmI) {
				(mod as CustomizerMod).guiOn = true;
                Main.playerInventory = true;
			}
			player.itemAnimation = 0;
			player.itemTime = 0;
			return true;
		}

		public override void AddRecipes ()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipeGroup("IronBar", 5);
			recipe.AddRecipeGroup("ItemCustomizer:AnyStrangePlant", 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}


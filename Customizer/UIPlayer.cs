using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraUI;
using TerraUI.Objects;
using TerraUI.Panels;
using TerraUI.Utilities;

namespace ItemCustomizer {
	public class UIPlayer : ModPlayer {
		bool canMove = false;
		Vector2 distanceVector = new Vector2();

		UITextBox nameBox;
		UIButton applyButton;
		UIButton closeButton;
		UIItemSlot itemSlot;
		UIItemSlot dyeSlot;

		UIPanel panel;

		public override bool Autoload(ref string name) {
			return true;
		}

		public override void Initialize() {
			byte objects = 4;
			int width = 300;
			int height = 22;
			int margin = 10;
			int y = margin;
			int x = 10;

			panel = new UIPanel(new Vector2((Main.screenWidth-(width + (x * 2)))/2, (((Main.screenHeight)-(objects*height)) - ((objects + 1) * y + 30))/2), new Vector2(width + (x * 2), (objects * height) + ((objects + 1) * y + 30)));
			nameBox = new UITextBox(new Vector2(x, y), new Vector2(width-(x*2), height), Main.fontItemStack, "", panel);
			closeButton = new UIButton(new Vector2(width-10, y), new Vector2(height, height), Main.fontItemStack, "X", parent: panel);
			y += height + margin;
			applyButton = new UIButton(new Vector2(x, y), new Vector2(width, height), Main.fontItemStack, "Apply", parent: panel);
			y += height + margin;
			itemSlot = new TerraUI.Objects.UIItemSlot(new Vector2(x, y), parent: panel);
			dyeSlot = new TerraUI.Objects.UIItemSlot(new Vector2(x + 56, y), context: Contexts.EquipDye, parent: panel);

			itemSlot.Click += itemSlot_Click;
			itemSlot.DrawItem = itemSlot_DrawItem;
			dyeSlot.DrawItem = itemSlot_DrawItem;

			closeButton.BackColor = new Color(151, 65, 85);
			closeButton.Click += close_Click;
			closeButton.MouseEnter += close_MouseEnter;
			closeButton.MouseLeave += close_MouseLeave;
			closeButton.MouseDown += close_MouseDown;
			closeButton.MouseUp += close_MouseUp;

			applyButton.Click += button_Click;
			applyButton.MouseEnter += button_MouseEnter;
			applyButton.MouseLeave += button_MouseLeave;
			applyButton.MouseDown += button_MouseDown;
			applyButton.MouseUp += button_MouseUp;

			nameBox.GotFocus += textbox_GotFocus;
			nameBox.LostFocus += textbox_LostFocus;

			//panel.MouseEnter += panel_MouseEnter;
			//panel.MouseLeave += panel_MouseLeave;
			panel.MouseDown += panel_GetMoveVector;
			panel.MouseUp += panel_StopMoving;

			//bar.Maximum = 100;
			//bar.BarMargin = new Vector2(0, 5);

			base.Initialize();
		}

		private void panel_MouseLeave(UIObject sender, MouseEventArgs e) {
			((UIPanel)sender).BackColor = UIColors.BackColorTransparent;
		}

		private void panel_MouseEnter(UIObject sender, MouseEventArgs e) {
			((UIPanel)sender).BackColor = UIColors.LightBackColorTransparent;
		}

		private void textbox_LostFocus(UIObject sender) {
			((UITextBox)sender).BackColor = UIColors.TextBox.BackColor;
		}

		private void textbox_GotFocus(UIObject sender) {
			((UITextBox)sender).BackColor = Color.LightGoldenrodYellow;
		}

		private void button_MouseUp(UIObject sender, MouseButtonEventArgs e) {
			((UIButton)sender).BackColor = UIColors.LightBackColor;
		}

		private void button_MouseDown(UIObject sender, MouseButtonEventArgs e) {
			((UIButton)sender).BackColor = UIColors.DarkBackColor;
		}

		private void button_MouseLeave(UIObject sender, MouseEventArgs e) {
			((UIButton)sender).BackColor = UIColors.BackColor;
		}

		private void button_MouseEnter(UIObject sender, MouseEventArgs e) {
			((UIButton)sender).BackColor = UIColors.LightBackColor;
		}

		private void close_MouseUp(UIObject sender, MouseButtonEventArgs e) {
			((UIButton)sender).BackColor = new Color(190, 100, 100);
		}

		private void close_MouseDown(UIObject sender, MouseButtonEventArgs e) {
			((UIButton)sender).BackColor = new Color(151, 65, 65);
		}

		private void close_MouseLeave(UIObject sender, MouseEventArgs e) {
			((UIButton)sender).BackColor = new Color(151, 65, 85);
		}

		private void close_MouseEnter(UIObject sender, MouseEventArgs e) {
			((UIButton)sender).BackColor = new Color(190, 100, 100);
		}

		private bool close_Click(UIObject sender, MouseButtonEventArgs e){
			if(e.Button == MouseButtons.Left) {
				(mod as CustomizerMod).guiOn = false;
				return true;
			}
			return false;
		}

		private bool button_Click(UIObject sender, MouseButtonEventArgs e) {
			if(e.Button == MouseButtons.Left) {
				if(itemSlot.Item != null && itemSlot.Item.active) {
					CustomizerItemInfo info = itemSlot.Item.GetModInfo<CustomizerItemInfo>(mod);
					if(nameBox.Text != ""){
						itemSlot.Item.name = nameBox.Text;
					} else {
						Item dummy = new Item();
						dummy.SetDefaults(itemSlot.Item.type);
						itemSlot.Item.name = dummy.name;
					}
					info.itemName = nameBox.Text;

					if(dyeSlot.Item != null && dyeSlot.Item.active)
						info.shaderID = dyeSlot.Item.dye;
					else
						info.shaderID = 0;
					//Main.NewText(info.shaderID.ToString());
				}

				return true;
			}

			return false;
		}

		private bool itemSlot_Click(UIObject sender, MouseButtonEventArgs e){
			if(e.Button == MouseButtons.Left) {
				if(Main.mouseItem.type <= 0)
					goto SKIP;
				if(!(Main.mouseItem.maxStack == 1 || (Main.mouseItem.consumable && Main.mouseItem.createTile == -1 && Main.mouseItem.createWall == -1) || (Main.mouseItem.damage > 0 && Main.mouseItem.ammo == 0)))
					return true;
				SKIP:

				if(Main.mouseItem.type > 0)
					nameBox.Text = Main.mouseItem.name;
				else
					nameBox.Text = "";
			}
			return false;
		}

		private void itemSlot_DrawItem(UIObject sender, SpriteBatch spriteBatch){
			UIItemSlot slot = sender as UIItemSlot;

			Rectangle Rectangle = slot.Rectangle;

			Texture2D texture2D = Main.itemTexture[slot.Item.type];
			Rectangle rectangle2;

			if(Main.itemAnimations[slot.Item.type] != null) {
				rectangle2 = Main.itemAnimations[slot.Item.type].GetFrame(texture2D);
			}
			else {
				rectangle2 = texture2D.Frame(1, 1, 0, 0);
			}

			Vector2 origin = new Vector2(rectangle2.Width / 2, rectangle2.Height / 2);

			if(mod.GetGlobalItem("CustomizerItem").PreDrawInInventory(slot.Item, spriteBatch, new Vector2(Rectangle.X + Rectangle.Width / 2, Rectangle.Y + Rectangle.Height / 2), rectangle2, Color.White, Color.White, origin, 1f)) {
				spriteBatch.Draw(
					Main.itemTexture[slot.Item.type],
					new Vector2(Rectangle.X + Rectangle.Width / 2,
						Rectangle.Y + Rectangle.Height / 2),
					new Rectangle?(rectangle2),
					Color.White,
					0f,
					origin,
					(slot.ScaleToInventory ? Main.inventoryScale : 1f),
					SpriteEffects.None,
					0f);
			}
			mod.GetGlobalItem("CustomizerItem").PostDrawInInventory(slot.Item, spriteBatch, new Vector2(Rectangle.X + Rectangle.Width / 2, Rectangle.Y + Rectangle.Height / 2), rectangle2, Color.White, Color.White, origin, 1f);
		}

		public override void PreUpdate() {
			if((mod as CustomizerMod).guiOn) {
				UpdateUI();
			} else {
				/*if(itemSlot.Item.active && player.ItemSpace(itemSlot.Item)) {
					player.GetItem(player.whoAmI, itemSlot.Item);
					itemSlot.Item = new Item();
				}
				if(dyeSlot.Item.active && player.ItemSpace(dyeSlot.Item)) {
					player.GetItem(player.whoAmI, dyeSlot.Item);
					dyeSlot.Item = new Item();
				}*/
				if(!itemSlot.Item.active) nameBox.Text = "";
				else nameBox.Text = itemSlot.Item.name;
			}
		}

		public void DrawUI(SpriteBatch spriteBatch) {
			if(panel != null)
				panel.Draw(spriteBatch);
		}

		public void UpdateUI() {
			if(!nameBox.Focused && Main.netMode < 2 && player.whoAmI == Main.myPlayer) {
				Main.clrInput();
			}
			UIUtils.UpdateInput();
			if(panel != null) {
				panel.Update();
				DoMovement();
			}
		}

		public void panel_GetMoveVector(UIObject sender, MouseButtonEventArgs e) 
		{
			if(e.Button == MouseButtons.Left) {
				if(UIUtils.NoChildrenIntersect(sender, new Rectangle(Main.mouseX, Main.mouseY, 1, 1))) {
					distanceVector = new Vector2(Main.mouseX, Main.mouseY) - sender.Position;
					canMove = true;
				}
			}
		}

		public void panel_StopMoving(UIObject sender, MouseButtonEventArgs e){
			if(e.Button == MouseButtons.Left) {
				canMove = false;
			}
		}

		public void DoMovement(){
			if(canMove) {
				panel.Position = new Vector2(Main.mouseX, Main.mouseY) - distanceVector;
			}
		}

		//Stops items from being deleted on world exit
		public override void SaveCustomData(BinaryWriter writer)
		{
			ItemIO.WriteItem(itemSlot.Item, writer, true, true);
			ItemIO.WriteItem(dyeSlot.Item, writer, false, true);
		}

		//Loads items back in on world entry
		public override void LoadCustomData(BinaryReader reader)
		{
			itemSlot.Item = ItemIO.ReadItem(reader, true, true);
			dyeSlot.Item = ItemIO.ReadItem(reader, false, true);
		}
	}
}
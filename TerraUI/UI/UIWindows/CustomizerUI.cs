/*using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using ItemCustomizer.UI;
using ItemCustomizer.UI.Windows;

namespace ItemCustomizer.UI.Windows {
    public class CustomizerUI {
        private Vector2 distance;

        public UIObject Window { get; set; }
        public bool CanMove { get; set; }

        /// <summary>
        /// Create a new example window.
        /// </summary>
        public CustomizerUI() {
            Window = new UIPanel(
                new Vector2(Main.screenWidth / 2 - 150,
                            Main.screenHeight / 2 - 75),
                new Vector2(300, 150),
                true);

			UIItemSlot itemSlot = new UIItemSlot(new Vector2(30, 80), 52, Contexts.InventoryItem, Window,
                                      delegate(Item item) {
                    //Allow grabbing items with empty hand
                    if(item.type <= 0)
                        return true;

                    //Only accept unstackable items to prevent bugs
                    if(item.maxStack == 1)
                        return true;

                    return false;
                }, null, null, null, true);
			UIItemSlot dyeSlot = new UIItemSlot(new Vector2(70, 80), 52, Contexts.EquipDye, Window, 
                                     delegate(Item item) {
                    //Allow grabbing items with empty hand
                    if(item.type <= 0)
                        return true;

                    //Only accept dyes
                    if(item.dye > 0)
                        return true;

                    return false;
                }, null, null, null, true);

            UITextBox nameBox = new UITextBox(new Vector2(30, 120), new Vector2(100, 32), Main.fontMouseText, "", Window);
			//nameBox.Click += Focus_Left_Click;

            UIButton applyButton = new UIButton(new Vector2(30, 30), new Vector2(70, 40), Main.fontMouseText, "Apply",
                                  1, null, Window); 
			applyButton.Click += new MouseClickEventHandler(delegate(UIObject sender, MouseButtonEventArgs e) {
				if(e.Button == MouseButtons.Left) {
					CustomizerItemInfo info = itemSlot.Item.GetModInfo<CustomizerItemInfo>(UIUtils.Mod);
					Item dummy = new Item();
					dummy.SetDefaults(itemSlot.Item.type);
					if(nameBox.Text == dummy.name || nameBox.Text == "") {
						info.itemName = "";
						itemSlot.Item.name = dummy.name;
						goto END;
					}

					info.itemName = nameBox.Text;
					itemSlot.Item.name = nameBox.Text;

					END:
					nameBox.Text = "";

					info.shaderID = dyeSlot.Item == null ? 0 : dyeSlot.Item.dye;

					return true;
				}
				return false;
			});

            Window.Children.Add(itemSlot);
            Window.Children.Add(dyeSlot);
            Window.Children.Add(nameBox);
            Window.Children.Add(applyButton);
            Window.Click += Window_Click;
        }

		private bool Focus_Left_Click(UIObject sender, MouseButtonEventArgs e) {
			if(e.Button == MouseButtons.Left) {
				sender.Focus();
				return true;
			}
			return false;
		}

        private bool Window_Click(UIObject sender, MouseButtonEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                // do something
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draw the window. Call this during some Draw() function.
        /// </summary>
        /// <param name="sb">drawing SpriteBatch</param>
        public void Draw(SpriteBatch sb) {
			try{
				Window.Draw(sb);
			} catch(Exception e){
				ErrorLogger.Log(e.ToString());
			}
        }

        /// <summary>
        /// Update the window. Call this during some Update() function.
        /// </summary>
        public void Update() {
            DoMovement();
            Window.Update();
        }

        /// <summary>
        /// Move the window.
        /// </summary>
        private void DoMovement() {
            Rectangle oldMouseRect = new Rectangle(MouseUtils.LastState.X, MouseUtils.LastState.Y, 1, 1);

            if(oldMouseRect.Intersects(Window.Rectangle) &&
               MouseUtils.Rectangle.Intersects(Window.Rectangle) &&
               UIUtils.NoChildrenIntersect(Window, MouseUtils.Rectangle)) {
                if(MouseUtils.State.LeftButton == ButtonState.Pressed) {
                    distance = MouseUtils.Position - Window.Position;
                    CanMove = true;
                }
            }

            if(CanMove) {
                Window.Position = new Vector2(Main.mouseX, Main.mouseY) - distance;
            }

            if(MouseUtils.State.LeftButton == ButtonState.Released) {
                CanMove = false;
            }
        }
    }
}*/

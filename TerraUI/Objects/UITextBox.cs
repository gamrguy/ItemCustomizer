using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria;
using TerraUI;
using TerraUI.Utilities;
using TerraUI.Objects;

namespace TerraUI.Objects {
    public class UITextBox : UIObject {
        private const int frameDelay = 9;
        private int selectionStart = 0;
        private int leftArrow = 0;
        private int rightArrow = 0;
        private int delete = 0;

        /// <summary>
        /// The text displayed in the UITextBox.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The font used in the UITextBox.
        /// </summary>
        public SpriteFont Font { get; set; }
        /// <summary>
        /// The default border color.
        /// </summary>
        public Color BorderColor { get; set; }
        /// <summary>
        /// The default background color.
        /// </summary>
        public Color BackColor { get; set; }
        /// <summary>
        /// The default text color.
        /// </summary>
        public Color TextColor { get; set; }
        /// <summary>
        /// The index where the selection in the UITextBox begins.
        /// </summary>
        public int SelectionStart {
            get { return selectionStart; }
            private set {
                if(value < 0) {
                    selectionStart = 0;
                }
                else if(value > Text.Length) {
                    selectionStart = Text.Length;
                }
                else {
                    selectionStart = value;
                }
            }
        }

        /// <summary>
        /// Create a new UITextBox.
        /// </summary>
        /// <param name="position">position of object in pixels</param>
        /// <param name="size">size of object in pixels</param>
        /// <param name="font">text font</param>
        /// <param name="text">displayed text</param>
        /// <param name="parent">parent object</param>
        public UITextBox(Vector2 position, Vector2 size, SpriteFont font, string text = "", UIObject parent = null)
            : base(position, size, parent, true, true) {
            Text = text;
            Focused = false;
            Font = font;
            BorderColor = UIColors.TextBox.BorderColor;
            BackColor = UIColors.TextBox.BackColor;
            TextColor = UIColors.TextBox.TextColor;
        }

        /// <summary>
        /// Give the object focus.
        /// </summary>
        public override void Focus() {
            base.Focus();
			SelectionStart = Text.Length;
        }

        /// <summary>
        /// Update the object. Call during any Update() function.
        /// </summary>
        public override void Update() {

            if(Focused) {
                bool skip = false;

                if(Text.Length > 0) {
					if(KeyboardUtils.JustPressed(Keys.Left) || KeyboardUtils.HeldDown(Keys.Left)) {
                        if(leftArrow == 0) {
                            SelectionStart--;
                            leftArrow = frameDelay;
                        }
                        leftArrow--;
                        skip = true;
                    }
					else if(KeyboardUtils.JustPressed(Keys.Right) || KeyboardUtils.HeldDown(Keys.Right)) {
                        if(rightArrow == 0) {
                            SelectionStart++;
                            rightArrow = frameDelay;
                        }
                        rightArrow--;
                        skip = true;
					}
					else if((KeyboardUtils.JustPressed(Keys.Delete) || KeyboardUtils.HeldDown(Keys.Delete))
						|| (KeyboardUtils.JustPressed(Keys.Back) || KeyboardUtils.HeldDown(Keys.Back))) {
						if(delete == 0) {
                            if(SelectionStart > 0) {
                                Text = Text.Remove(SelectionStart-1, 1);
								SelectionStart--;
                            }
                            delete = frameDelay;
                        }
                        delete--;
                        skip = true;
                    }
					else if((KeyboardUtils.JustPressed(Keys.Enter) || KeyboardUtils.HeldDown(Keys.Enter))
						|| (KeyboardUtils.JustPressed(Keys.Escape) || KeyboardUtils.HeldDown(Keys.Escape))) {
                        Unfocus();
						//Main.NewText("Pressed Enter, Focused: " + Focused);
						Main.drawingPlayerChat = false;
						//Main.playerInventory = true;
                    }
                    else {
                        leftArrow = 0;
                        rightArrow = 0;
                        delete = 0;
                    }
                }

                if(!skip) {
                    int oldLength = Text.Length;
                    string substring = Text.Substring(0, SelectionStart);
					//Main.hasFocus = true;
                    string input = GetInputText(substring);

                    // first, we check if the length of the string has changed, indicating
                    // text has been added or removed
                    if(input.Length != substring.Length) {
                        // we remove the old text and replace it with the new, storing it
                        // in a temporary variable
                        string newText = Text.Remove(0, SelectionStart).Insert(0, input);

                        // now if the text is smaller than previously or if not, the string is
                        // an appropriate size,
                        if(newText.Length < Text.Length || Font.MeasureString(newText).X < Size.X - 12) {
                            // we set the old text to the new text
                            Text = newText;

                            // if the length of the text is now longer,
                            if(Text.Length > oldLength) {
                                // adjust the selection start accordingly
                                SelectionStart += (Text.Length - oldLength);
                            }
                            // or if the length of the text is now shorter
                            else if(Text.Length < oldLength) {
                                // adjust the selection start accordingly
                                SelectionStart -= (oldLength - Text.Length);
                            }
                        }
                    }
                }
            }

            base.Update();
        }

        /// <summary>
        /// Draw the UITextBox.
        /// </summary>
        /// <param name="spriteBatch">drawing SpriteBatch</param>
        public override void Draw(SpriteBatch spriteBatch) {
            Rectangle = new Rectangle((int)RelativePosition.X, (int)RelativePosition.Y, (int)Size.X, (int)Size.Y);

            BaseTextureDrawing.DrawRectangleBox(spriteBatch, BorderColor, BackColor, Rectangle, 2);

            if(Focused) {
                spriteBatch.DrawString(Font, Text.Insert(SelectionStart, "|"), RelativePosition + new Vector2(2), TextColor);
            }
            else {
                spriteBatch.DrawString(Font, Text, RelativePosition + new Vector2(2), TextColor);
            }

            base.Draw(spriteBatch);
        }

		//The vanilla text input method, modified slightly.
		//Please ignore this. It is, of course, ugly vanilla code.
		public string GetInputText(string oldString)
		{
			//if (!Main.hasFocus)
			//{
			//	return oldString;
			//}
			Main.inputTextEnter = false;
			Main.inputTextEscape = false;
			string text = oldString;
			string newKeys = "";
			if (text == null)
			{
				text = "";
			}
			//bool flag = false;
			if (Main.inputText.IsKeyDown(Keys.LeftControl) || Main.inputText.IsKeyDown(Keys.RightControl))
			{
				if (KeyboardUtils.JustPressed(Keys.Z))
				{
					//text = "";
				}
				else if (KeyboardUtils.JustPressed(Keys.X))
				{
					//PlatformUtilties.SetClipboard(oldString);
					//System.Windows.Forms.Clipboard.SetText(oldString);
					//text = "";
				}
				else if (KeyboardUtils.JustPressed(Keys.C) || KeyboardUtils.JustPressed(Keys.Insert))
				{
					//PlatformUtilties.SetClipboard(oldString);
					//System.Windows.Forms.Clipboard.SetText(oldString);
				}
				else if (KeyboardUtils.JustPressed(Keys.V))
				{
					//Main.<>c__DisplayClass1c expr_13B = <>c__DisplayClass1c;
					//newKeys += PlatformUtilties.GetClipboard();
					//newKeys += System.Windows.Forms.Clipboard.GetText();
				}
			}
			else
			{
				if (Main.inputText.IsKeyDown(Keys.LeftShift) || Main.inputText.IsKeyDown(Keys.RightShift))
				{
					if (KeyboardUtils.JustPressed(Keys.Delete))
					{
						/*Thread thread = new Thread(delegate()
							{
								if (oldString.Length > 0)
								{
									if(Environment.OSVersion.Platform == PlatformID.MacOSX){
										using (Process p = new Process()){
											p.StartInfo = new ProcessStartInfo("pbcopy", "-pboard general -Prefer txt");
											p.StartInfo.UseShellExecute = false;
											p.StartInfo.RedirectStandardOutput = false;
											p.StartInfo.RedirectStandardInput = true;
											p.Start();
											p.StandardInput.Write(oldString);
											p.StandardInput.Close();
											p.WaitForExit();
										}
									} else if(Environment.OSVersion.Platform == PlatformID.Unix){
										using (Process p = new Process()){
											//p.StartInfo = new ProcessStartInfo("
										}
									} else {
										System.Windows.Forms.Clipboard.SetText(oldString);
									}
								}
							});
						thread.SetApartmentState(ApartmentState.STA);
						thread.Start();
						while (thread.IsAlive)
						{
						}
						text = "";*/
					}
					if (KeyboardUtils.JustPressed(Keys.Insert))
					{
						/*Thread thread2 = new Thread(delegate()
							{
								string text2 = System.Windows.Forms.Clipboard.GetText();
								for (int l = 0; l < text2.Length; l++)
								{
									if (text2[l] < ' ' || text2[l] == '\u007f')
									{
										text2 = text2.Replace(string.Concat(text2[l--]), "");
									}
								}
								newKeys += text2;
							});
						thread2.SetApartmentState(ApartmentState.STA);
						thread2.Start();
						while (thread2.IsAlive)
						{
						}*/
					}
				}
				for (int i = 0; i < Main.keyCount; i++)
				{
					int num = Main.keyInt[i];
					string str = Main.keyString[i];
					if (num == 13)
					{
						Main.inputTextEnter = true;
					}
					else if (num == 27)
					{
						Main.inputTextEscape = true;
					}
					else if (num >= 32 && num != 127)
					{
						//Main.<>c__DisplayClass1c expr_25B = <>c__DisplayClass1c;
						newKeys += str;
					}
				}
			}
			Main.keyCount = 0;
			text += newKeys;
			Main.oldInputText = Main.inputText;
			Main.inputText = Keyboard.GetState();
			/*Keys[] pressedKeys = Main.inputText.GetPressedKeys();
			Keys[] pressedKeys2 = Main.oldInputText.GetPressedKeys();
			if (KeyboardUtils.HeldDown(Keys.Back))
			{
				if (backSpaceCount == 0)
				{
					backSpaceCount = 7;
					flag = true;
				}
				backSpaceCount--;
			}
			else
			{
				backSpaceCount = 15;
			}
			for (int j = 0; j < pressedKeys.Length; j++)
			{
				bool flag2 = true;
				for (int k = 0; k < pressedKeys2.Length; k++)
				{
					if (pressedKeys[j] == pressedKeys2[k])
					{
						flag2 = false;
					}
				}
				string a = string.Concat(pressedKeys[j]);
				if (a == "Back" && (flag2 || flag) && text.Length > 0)
				{
					TextSnippet[] array = ChatManager.ParseMessage(text, Color.White);
					if (array[array.Length - 1].DeleteWhole)
					{
						text = text.Substring(0, text.Length - array[array.Length - 1].TextOriginal.Length);
					}
					else
					{
						text = text.Substring(0, text.Length - 1);
					}
				}
			}*/
			return text;
		}
    }
}

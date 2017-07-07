using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.UI;
using ReLogic.Graphics;

namespace ItemCustomizer
{
	public class CustomizerUI : UIState
	{
		public UIPanel customizerPanel;
		public UIItemSlot itemSlot;
		public UIItemSlot dyeSlot;
		public NewUITextBox textBox;
		public static bool visible = false;

		public override void OnInitialize()
		{
			int width = 240;
			int height = 140;

			customizerPanel = new UIPanel();
			customizerPanel.SetPadding(0);
			SetPosition(customizerPanel, (Main.screenHeight - height) / 2, (Main.screenWidth - width) / 2, width, height);
			customizerPanel.BackgroundColor = new Color(63, 82, 151) * 0.7f;

			customizerPanel.OnMouseDown += DragStart;
			customizerPanel.OnMouseUp += DragEnd;

			UIClassicTextButton applyButton = new UIClassicTextButton("Apply");
			//SetPosition(applyButton, height - 30, 10, 40, 20);
			applyButton.Top.Set(height - 30, 0);
			applyButton.Left.Set(10, 0);
			applyButton.OnClick += ApplyButtonClicked;
			customizerPanel.Append(applyButton);

			UIClassicTextButton closeButton = new UIClassicTextButton("Close");
			//SetPosition(closeButton, height - 30, width - 50, 40, 20);
			closeButton.Top.Set(height - 30, 0);
			closeButton.Left.Set(width - 50, 0);
			closeButton.OnClick += CloseButtonClicked;
			customizerPanel.Append(closeButton);

			UIClassicTextButton resetButton = new UIClassicTextButton("Reset");
			//SetPosition(resetButton, height - 30, (width/2) - 20, 40, 20);
			resetButton.Top.Set(height - 30, 0);
			resetButton.Left.Set(width / 2 - 20, 0);
			resetButton.OnClick += ResetButtonClicked;
			customizerPanel.Append(resetButton);

			dyeSlot = new UIItemSlot(ItemSlot.Context.EquipDye);
			SetPosition(dyeSlot, height / 2 - 30, width / 3 - 30, 60, 60);
			customizerPanel.Append(dyeSlot);

			itemSlot = new UIItemSlot(ItemSlot.Context.ChestItem);
			SetPosition(itemSlot, height / 2 - 30, width * (2.0f / 3.0f) - 30, 60, 60);
			customizerPanel.Append(itemSlot);

			textBox = new NewUITextBox("Rename...");
			SetPosition(textBox, 10, 10, width - 20, 20);
			customizerPanel.Append(textBox);

			Append(customizerPanel);
		}

		private void ApplyButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			if(itemSlot.item != null && itemSlot.item.active && !itemSlot.item.IsAir) {
				CustomizerItem info = itemSlot.item.GetGlobalItem<CustomizerItem>(CustomizerMod.mod);
				if(textBox.Text != "") {
					itemSlot.item.SetNameOverride(textBox.Text);
				} else {
					itemSlot.item.ClearNameOverride();
				}
				info.itemName = textBox.Text;

				if(dyeSlot.item != null && dyeSlot.item.active && !dyeSlot.item.IsAir) {
					info.shaderID = GameShaders.Armor.GetShaderIdFromItemId(dyeSlot.item.type);
				} else {
					info.shaderID = 0;
				}
			}

			Main.PlaySound(SoundID.Grab);
		}

		private void ResetButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			if(itemSlot.item != null && itemSlot.item.active && !itemSlot.item.IsAir) {
				CustomizerItem info = itemSlot.item.GetGlobalItem<CustomizerItem>(CustomizerMod.mod);
				info.itemName = "";
				info.shaderID = 0;
				itemSlot.item.ClearNameOverride();
			}
			textBox.SetText("");

			Main.PlaySound(SoundID.Grab);
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuClose);
			visible = false;
		}

		private void ItemSlotClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			if(itemSlot.item != null && itemSlot.item.active && !itemSlot.item.IsAir) {
				CustomizerItem info = itemSlot.item.GetGlobalItem<CustomizerItem>(CustomizerMod.mod);
				if(info.itemName != "") textBox.SetText(info.itemName);
				else textBox.SetText("");
			} else textBox.SetText("");
		}

		Vector2 offset;
		public bool dragging = false;
		private void DragStart(UIMouseEvent evt, UIElement listeningElement)
		{
			offset = new Vector2(evt.MousePosition.X - customizerPanel.Left.Pixels, evt.MousePosition.Y - customizerPanel.Top.Pixels);
			dragging = true;
		}

		private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
		{
			Vector2 end = evt.MousePosition;
			dragging = false;

			customizerPanel.Left.Set(end.X - offset.X, 0f);
			customizerPanel.Top.Set(end.Y - offset.Y, 0f);

			Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if(customizerPanel.ContainsPoint(MousePosition)) {
				Main.LocalPlayer.mouseInterface = true;
			}
			if(dragging) {
				customizerPanel.Left.Set(MousePosition.X - offset.X, 0f);
				customizerPanel.Top.Set(MousePosition.Y - offset.Y, 0f);
				Recalculate();
			}
		}

		private void SetPosition(UIElement element, float top, float left, float width, float height, float perc = 0)
		{
			element.Top.Set(top, perc);
			element.Left.Set(left, perc);
			element.Width.Set(width, perc);
			element.Height.Set(height, perc);
		}
	}

	/// <summary>
	/// A text box that works. Yeah.
	/// Yes, before you ask, I stole this from jopojelly. Yay for open-source code!
	/// </summary>
	public class NewUITextBox : UITextPanel<string>
	{
		internal bool focused = false;
		private int _cursor;
		private int _frameCount;
		private int _maxLength = 60;
		private string hintText;
		public event Action OnFocus;
		public event Action OnUnfocus;
		public event Action OnTextChanged;
		public event Action OnTabPressed;
		public event Action OnEnterPressed;
		public event Action OnUpPressed;
		internal bool unfocusOnEnter = true;
		internal bool unfocusOnTab = true;


		public NewUITextBox(string text, float textScale = 1, bool large = false) : base("", textScale, large)
		{
			hintText = text;
			SetPadding(0);
			//			keyBoardInput.newKeyEvent += KeyboardInput_newKeyEvent;
		}

		public override void Click(UIMouseEvent evt)
		{
			Focus();
			base.Click(evt);
		}

		public void SetUnfocusKeys(bool unfocusOnEnter, bool unfocusOnTab)
		{
			this.unfocusOnEnter = unfocusOnEnter;
			this.unfocusOnTab = unfocusOnTab;
		}

		//void KeyboardInput_newKeyEvent(char obj)
		//{
		//	// Problem: keyBoardInput.newKeyEvent only fires on regular keyboard buttons.

		//	if (!focused) return;
		//	if (obj.Equals((char)Keys.Back)) // '\b'
		//	{
		//		Backspace();
		//	}
		//	else if (obj.Equals((char)Keys.Enter))
		//	{
		//		Unfocus();
		//		Main.chatRelease = false;
		//	}
		//	else if (Char.IsLetterOrDigit(obj))
		//	{
		//		Write(obj.ToString());
		//	}
		//}

		public void Unfocus()
		{
			if(focused) {
				focused = false;
				Main.blockInput = false;

				OnUnfocus?.Invoke();
			}
		}

		public void Focus()
		{
			if(!focused) {
				Main.clrInput();
				focused = true;
				Main.blockInput = true;

				OnFocus?.Invoke();
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if(!ContainsPoint(MousePosition) && Main.mouseLeft) {
				// TODO, figure out how to refocus without triggering unfocus while clicking enable button.
				Unfocus();
			}
			base.Update(gameTime);
		}

		public void Write(string text)
		{
			base.SetText(base.Text.Insert(this._cursor, text));
			this._cursor += text.Length;
			_cursor = Math.Min(Text.Length, _cursor);
			Recalculate();

			OnTextChanged?.Invoke();
		}

		public void WriteAll(string text)
		{
			bool changed = text != Text;
			base.SetText(text);
			this._cursor = text.Length;
			//_cursor = Math.Min(Text.Length, _cursor);
			Recalculate();

			if(changed) {
				OnTextChanged?.Invoke();
			}
		}

		public override void SetText(string text, float textScale, bool large)
		{
			if(text.ToString().Length > this._maxLength) {
				text = text.ToString().Substring(0, this._maxLength);
			}
			base.SetText(text, textScale, large);

			//this.MinWidth.Set(120, 0f);

			this._cursor = Math.Min(base.Text.Length, this._cursor);

			OnTextChanged?.Invoke();
		}

		public void SetTextMaxLength(int maxLength)
		{
			this._maxLength = maxLength;
		}

		public void Backspace()
		{
			if(this._cursor == 0) {
				return;
			}
			base.SetText(base.Text.Substring(0, base.Text.Length - 1));
			Recalculate();
		}

		public void CursorLeft()
		{
			if(this._cursor == 0) {
				return;
			}
			this._cursor--;
		}

		public void CursorRight()
		{
			if(this._cursor < base.Text.Length) {
				this._cursor++;
			}
		}

		static bool JustPressed(Keys key)
		{
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(focused) {
				Terraria.GameInput.PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				// This might work.....assuming chat isn't open
				WriteAll(Main.GetInputText(Text));

				if(JustPressed(Keys.Tab)) {
					if(unfocusOnTab) Unfocus();
					//	Main.NewText("Tab");
					OnTabPressed?.Invoke();
				}

				if(JustPressed(Keys.Enter)) {
					//	Main.NewText("Enter");
					if(unfocusOnEnter) Unfocus();
					OnEnterPressed?.Invoke();
				}
				if(JustPressed(Keys.Up)) {
					OnUpPressed?.Invoke();
				}

			}
			CalculatedStyle innerDimensions2 = base.GetInnerDimensions();
			Vector2 pos2 = innerDimensions2.Position();
			if(IsLarge) {
				pos2.Y -= 10f * TextScale * TextScale;
			} else {
				pos2.Y -= 2f * TextScale;
			}
			//pos2.X += (innerDimensions2.Width - TextSize.X) * 0.5f;
			if(IsLarge) {
				Utils.DrawBorderStringBig(spriteBatch, Text, pos2, TextColor, TextScale, 0f, 0f, -1);
				return;
			}
			Utils.DrawBorderString(spriteBatch, Text, pos2, TextColor, TextScale, 0f, 0f, -1);

			this._frameCount++;

			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();
			DynamicSpriteFont spriteFont = base.IsLarge ? Main.fontDeathText : Main.fontMouseText;
			Vector2 vector = new Vector2(spriteFont.MeasureString(base.Text.Substring(0, this._cursor)).X, base.IsLarge ? 32f : 16f) * base.TextScale;
			if(base.IsLarge) {
				pos.Y -= 8f * base.TextScale;
			} else {
				pos.Y -= 1f * base.TextScale;
			}
			if(Text.Length == 0) {
				Vector2 hintTextSize = new Vector2(spriteFont.MeasureString(hintText.ToString()).X, IsLarge ? 32f : 16f) * TextScale;
				pos.X += 5;//(hintTextSize.X);
				if(base.IsLarge) {
					Utils.DrawBorderStringBig(spriteBatch, hintText, pos, Color.Gray, base.TextScale, 0f, 0f, -1);
					return;
				}
				Utils.DrawBorderString(spriteBatch, hintText, pos, Color.Gray, base.TextScale, 0f, 0f, -1);
				pos.X -= 5;
				//pos.X -= (innerDimensions.Width - hintTextSize.X) * 0.5f;
			}

			if(!focused) return;

			pos.X += /*(innerDimensions.Width - base.TextSize.X) * 0.5f*/ +vector.X - (base.IsLarge ? 8f : 4f) * base.TextScale + 6f;
			if((this._frameCount %= 40) > 20) {
				return;
			}
			if(base.IsLarge) {
				Utils.DrawBorderStringBig(spriteBatch, "|", pos, base.TextColor, base.TextScale, 0f, 0f, -1);
				return;
			}
			Utils.DrawBorderString(spriteBatch, "|", pos, base.TextColor, base.TextScale, 0f, 0f, -1);
		}
	}

	/// <summary>
	/// A UIElement for an item slot, because vanilla doesn't have one.
	/// </summary>
	public class UIItemSlot : UIElement 
	{
		public Texture2D backgroundTexture = Main.inventoryBackTexture;
		public Item item;
		public int context;
		public float scale = 1.0f;

		public UIItemSlot(int c = 0)
		{
			context = c;
			OnClick += DefaultClickAction;
			OnRightClick += DefaultRightClickAction;

			item = new Item();
			item.active = false;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();

			spriteBatch.Draw(backgroundTexture, innerDimensions.Position(), default(Color));

			float oldScale = Main.inventoryScale;
			Main.inventoryScale = scale;
			ItemSlot.Draw(spriteBatch, ref item, context, new Vector2(innerDimensions.X, innerDimensions.Y));
			Main.inventoryScale = oldScale;

			bool isHovering = Main.mouseX >= innerDimensions.X && Main.mouseX - innerDimensions.X <= Width.Pixels &&
			 				  Main.mouseY >= innerDimensions.Y && Main.mouseY - innerDimensions.Y <= Height.Pixels;
			if(isHovering) { 
				Main.hoverItemName = item.HoverName;
				Main.HoverItem = item;
			}
		}

		private void DefaultClickAction(UIMouseEvent evt, UIElement listeningElement)
		{
			ItemSlot.LeftClick(ref item, context);
		}

		private void DefaultRightClickAction(UIMouseEvent evt, UIElement listeningElement)
		{
			ItemSlot.RightClick(ref item, context);
		}
	}

	/// <summary>
	/// A UIElement designed to replicate Terraria's text buttons.
	/// The text will turn yellow and enlarge in size when hovered over.
	/// </summary>
	public class UIClassicTextButton : UIElement
	{
		public int hoverSound = SoundID.MenuTick;  //Sound that plays when hovering over the button
		public string text;          //this button's text
		public Color textColor;      //color of text
		public Color hoverColor;     //color text turns into
		public Color outlineColor;   //color of text outline
		public float minScale;       //minimum text scale
		public float maxScale;       //maximum text scale
		public float speed;          //frames until max scale

		public bool isHovering;      //whether the mouse is currently hovering over this button
		public float textScale;      //the current text scaling

		public UIClassicTextButton() : this("")
		{
			
		}

		public UIClassicTextButton(string text) : this(text, Color.White, Color.Yellow, Color.Black)
		{
			
		}

		public UIClassicTextButton(string str, Color textC, Color hoverC, Color outlineC, float min = 1.0f, float max = 1.3f, int spd = 20)
		{
			text = str;
			textColor = textC;
			hoverColor = hoverC;
			outlineColor = outlineC;
			minScale = min;
			maxScale = max;
			textScale = minScale;
			speed = spd;

			Vector2 dim = Main.fontMouseText.MeasureString(str) * minScale;
			Width.Set(dim.X, 0);
			Height.Set(dim.Y, 0);
		}

		public override void Update(GameTime gameTime)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			bool justStartedHovering = isHovering;
			isHovering = Main.mouseX >= innerDimensions.X && Main.mouseX - innerDimensions.X <= Width.Pixels && 
			             Main.mouseY >= innerDimensions.Y && Main.mouseY - innerDimensions.Y <= Height.Pixels;
			justStartedHovering = isHovering && !justStartedHovering ? true : false;
			float mult = (gameTime.ElapsedGameTime.Milliseconds / 1000.0f) * 60.0f; //speed stuff up when game is slow
			textScale += isHovering ? mult * ((maxScale - minScale) / speed) : mult * (-(maxScale - 1.0f) / speed);
			textScale = textScale > maxScale ? maxScale : textScale;
			textScale = textScale < minScale ? minScale : textScale;

			if(justStartedHovering) Main.PlaySound(hoverSound);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Color color = isHovering ? hoverColor : textColor;
			Utils.DrawBorderStringFourWay(spriteBatch, Main.fontItemStack, text, innerDimensions.X + Width.Pixels / 2, innerDimensions.Y + Height.Pixels / 2, color, outlineColor, new Vector2(Width.Pixels, Height.Pixels) / 2, textScale);
		}
	}
}
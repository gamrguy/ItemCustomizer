using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using TerraUI;
using TerraUI.Objects;
using TerraUI.Panels;
using TerraUI.Utilities;

namespace ItemCustomizer {
    public class ExampleWindow {
        private Vector2 distance;

        public UIObject Window { get; set; }
        public bool CanMove { get; set; }

        /// <summary>
        /// Create a new example window.
        /// </summary>
        public ExampleWindow() {
            Window = new UIPanel(
                new Vector2(Main.screenWidth / 2 - 150,
                            Main.screenHeight / 2 - 75),
                new Vector2(300, 150),
                true);

            // Window.Children.Add(something);
            Window.Click += Window_Click;
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
            Window.Draw(sb);
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
}

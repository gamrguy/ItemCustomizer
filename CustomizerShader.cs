using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderLib;
using Terraria;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	class CustomizerShader : GlobalShader
	{
		public override int ItemInventoryShader(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			return item.Customizer().shaderID.ID;
		}

		public override int ItemWorldShader(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return item.Customizer().shaderID.ID;
		}

		public override int ProjectileShader(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			return projectile.Customizer().shaderID;
		}

		public override void HeldItemShader(ref int shaderID, Item item, PlayerDrawInfo drawInfo) {
			shaderID = shaderID > 0 ? shaderID : item.Customizer().shaderID.ID;
		}

		public override int NPCShader(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			return npc.Customizer().shaderID;
		}
	}
}

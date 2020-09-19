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
			// Failsafe for items that haven't been initialized properly, if that's even a thing
			if(item.Customizer() is CustomizerItem customizer) return customizer.shaderID.ID;
			else return 0;
		}

		public override int ItemWorldShader(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			// Failsafe for items that haven't been initialized properly, if that's even a thing
			if(item.Customizer() is CustomizerItem customizer) return customizer.shaderID.ID;
			else return 0;
		}

		public override int ProjectileShader(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			// Failsafe for projectiles that haven't been initialized properly, if that's even a thing
			if(projectile.Customizer() is CustomizerProjInfo customizer) return customizer.shaderID;
			else return 0;
		}

		public override void HeldItemShader(ref int shaderID, Item item, PlayerDrawInfo drawInfo) {
			shaderID = shaderID > 0 ? shaderID : item.Customizer().shaderID.ID;
		}

		public override int NPCShader(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			return npc.Customizer().shaderID;
		}
	}
}

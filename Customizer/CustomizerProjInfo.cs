using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerProjInfo : GlobalProjectile
	{
		public bool parent = true; //Whether this is a parent projectile.
		public int shaderID = -1; //The shader being applied to this projectile

		public override bool InstancePerEntity {
			get {
				return true;
			}
		}
	}
}

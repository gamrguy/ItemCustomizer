using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerNPCInfo : GlobalNPC
	{
		public int shaderID = -1; //The shader being applied to this NPC

		public override bool InstancePerEntity => true;
	}
}

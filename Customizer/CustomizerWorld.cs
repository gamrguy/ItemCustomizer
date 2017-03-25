/*using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public class CustomizerWorld : ModDust
	{
		//public Dictionary<int, int> newProjs = new Dictionary<int, int>();
		//public Dictionary<int, Tuple<int, int>> newDusts = new Dictionary<int, Tuple<int, int>>();

		//This runs after all projectile AI, NPC AI, drawing, etc..
		public override void PostUpdate()
		{
			foreach(KeyValuePair<int, int> pair in newProjs) {
				CustomizerProjInfo info = Main.projectile[pair.Key].GetModInfo<CustomizerProjInfo>(mod);
				info.shaderID = pair.Value;
			}
			foreach(KeyValuePair<int, Tuple<int, int>> pair in newDusts) {
				Dust dust = Main.dust[pair.Key];
				dust.shader = GameShaders.Armor.GetSecondaryShader(pair.Value.Item1, Main.player[pair.Value.Item2]);
			}
		}
	}
}

*/
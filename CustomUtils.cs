using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemCustomizer
{
	public static class CustomUtils
	{
		// Run this if net mode does NOT match
		public static void NetExclude(int mode, Action action) {
			if(Main.netMode != mode) action();
		}

		public static void SafeNameOverride(this Item item, string name) {
			if(name != "") item.SetNameOverride(name);
        }

		public static CustomizerItem Customizer(this Item item) {
			return item.GetGlobalItem<CustomizerItem>();
        }

		public static CustomizerProjInfo Customizer(this Projectile proj) {
			return proj.GetGlobalProjectile<CustomizerProjInfo>();
		}

		public static CustomizerNPCInfo Customizer(this NPC npc) {
			return npc.GetGlobalNPC<CustomizerNPCInfo>();
		}
	}
}
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace ItemCustomizer
{
	public class CustomizerProjInfo : ProjectileInfo
	{
		public bool parent = true; //Whether this is a parent projectile.
		public int shaderID = -1; //The shader being applied to this projectile
	}
}


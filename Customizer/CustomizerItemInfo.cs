using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;

namespace ItemCustomizer
{
	public class CustomizerItemInfo : ItemInfo
	{
		public string itemName = "";       //This item's custom name
		public int shaderID = 0;           //This item's applied shader

		public bool customData {
			get {
				return !(itemName == "" && shaderID == 0);
			}
		}
	}
}


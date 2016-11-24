using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace ItemCustomizer
{
	public class CustomizerItemInfo : ItemInfo
	{
		public string itemName = "";       //This item's custom name
		public int shaderID = 0;           //This item's applied shader

		public bool customData { set{ customData = value; } get{ 
                return !(itemName == "" && shaderID == 0);
                } }
	}
}


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ID;
using ShaderLib.Dyes;

namespace ItemCustomizer
{
	public class CustomizerItemInfo : ItemInfo
	{
		/*
		public override ItemInfo Clone()
		{
			var clone = new CustomizerItemInfo();
			clone.itemdata = new Tuple<string, string>(itemdata.Item1, itemdata.Item2);

			clone.itemName = itemName;
			clone.shaderID = shaderID;
			return clone;
		}*/

		//public Tuple<string, string> itemdata = new Tuple<string, string>("", "");
		public string itemName = "";       //This item's custom name
		//This item's applied shader. Stored as item ID, accessed as shader ID.
		public int shaderID {
			get{ return shaderId; }
			set{ itemId = value; shaderId = GameShaders.Armor.GetShaderIdFromItemId(value); }
		}
		public int itemID {
			get{ return itemId; }
		}
		private int itemId = 0;
		private int shaderId = 0;

		public bool customData { 
			get{ return !(itemName == "" && shaderID == 0); }
		}
	}
}


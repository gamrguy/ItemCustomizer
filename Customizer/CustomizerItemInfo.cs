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
		public override ItemInfo Clone()
		{
			CustomizerItemInfo x = new CustomizerItemInfo();
			x.itemName = itemName;
			x.shaderID = shaderID;
			if(modDye != null) {
				x.modDye = new TagCompound();
				foreach(KeyValuePair<string, object> tag in modDye) {
					x.modDye.SetTag(tag.Key, tag.Value);
				}
			};

			return x;
		}

		public string itemName = "";       //This item's custom name
		public int shaderID = 0;           //This item's applied shader
		public TagCompound modDye;         //Modded dye, if applicable

		public bool customData { set{ customData = value; } get{ 
                return !(itemName == "" && shaderID == 0);
                } }
	}
}


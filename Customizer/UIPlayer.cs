using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ItemCustomizer {
	public class UIPlayer : ModPlayer {

		//Stops items from being deleted on world exit
		public override TagCompound Save()
		{
			CustomizerMod realMod = mod as CustomizerMod;
			return new TagCompound{
				{"ItemSlot", ItemIO.Save(realMod.customizerUI.itemSlot.item)},
				{"DyeSlot", ItemIO.Save(realMod.customizerUI.dyeSlot.item)}
			};
		}

		public override void Load(TagCompound tag)
		{
			CustomizerMod realMod = mod as CustomizerMod;
			realMod.customizerUI.itemSlot.item = ItemIO.Load(tag.GetCompound("ItemSlot"));
			realMod.customizerUI.dyeSlot.item = ItemIO.Load(tag.GetCompound("DyeSlot"));
		}

		//Old load function for compatibility reasons
		public override void LoadLegacy(BinaryReader reader)
		{
			CustomizerMod realMod = mod as CustomizerMod;
			ItemIO.LoadLegacy(realMod.customizerUI.itemSlot.item, reader, true, true);
			ItemIO.LoadLegacy(realMod.customizerUI.dyeSlot.item, reader, false, true);
		}
	}
}
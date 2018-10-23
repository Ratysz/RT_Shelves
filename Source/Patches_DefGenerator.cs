using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Harmony;

namespace RT_Shelves
{
	[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
	class Patch_GenerateImpliedDefs_PreResolve
	{
		public static bool Complete { get; private set; } = false;

		[HarmonyPriority(Priority.Last)]
		static bool Prefix()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Adding CompExtraSlots and generating upgrade defs:");
			foreach (var def in DefDatabase<ThingDef>.AllDefs.ToList().FindAll(x => x.thingClass.GetInterfaces().Contains(typeof(ISlotGroupParent))))
			{
				if (def.comps == null)
				{
					def.comps = new List<CompProperties>();
				}
				var comp = new CompProperties_ExtraSlots();
				switch (def.defName)
				{
					case "Shelf":
						comp.maxExtraSlots = 4;
						def.comps.Add(comp);
						break;
					default:
						def.comps.Add(comp);
						break;
				}
				DefDatabase<ThingDef>.Add(DefGen.BaseDef(def, false));
				DefDatabase<ThingDef>.Add(DefGen.BaseDef(def, true));
				builder.AppendLine($"    {def.defName} max extra slots: {comp.maxExtraSlots}");
			}
			Utility.Debug(builder.ToString());
			Complete = true;
			LoadedModManager.GetMod<Mod>().GetSettings<Mod.ModSettings>().ExposeData();
			return true;
		}
	}
}

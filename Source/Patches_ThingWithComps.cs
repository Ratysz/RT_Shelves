using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

using RimWorld;
using Verse;
using Harmony;

namespace RT_Shelves
{
	[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.GetGizmos))]
	class Patches_ThingWithComps
	{
		static void Postfix(ThingWithComps __instance, ref IEnumerable<Gizmo> __result)
		{
			if (__instance.Spawned)
			{
				var thingWithExtraSlots = __instance.Position.GetFirstThingWithComp<CompExtraSlots>(__instance.Map);
				if (thingWithExtraSlots != null && __instance != thingWithExtraSlots)
				{
					int extraSlots = thingWithExtraSlots.GetComp<CompExtraSlots>().currentExtraSlots;
					if (extraSlots > 0)
					{
						__result = AppendGizmo(__result, thingWithExtraSlots);
					}
				}
			}
		}

		static IEnumerable<Gizmo> AppendGizmo(IEnumerable<Gizmo> gizmos, ThingWithComps thing)
		{
			foreach (var gizmo in gizmos)
			{
				yield return gizmo;
			}
			yield return new Command_Action()
			{
				action = delegate
				{
					Find.Selector.ClearSelection();
					Find.Selector.Select(thing);
				},
				defaultLabel = thing.Label,
				defaultDesc = "Shortcut gizmo, so you don't have to click like 5 times to get to a shelf.", // TODO translation.
				icon = thing.def.uiIcon,
				iconAngle = thing.def.uiIconAngle,
				iconOffset = thing.def.uiIconOffset,
				iconProportions = thing.def.graphicData.drawSize.RotatedBy(thing.def.defaultPlacingRot),
				iconDrawScale = GenUI.IconDrawScale(thing.def),
				defaultIconColor = thing.Stuff.stuffProps.color,
			};
		}
	}
}

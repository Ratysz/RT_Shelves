using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

using RimWorld;
using Verse;
using Harmony;
using Verse.AI;

namespace RT_Shelves
{
	[HarmonyPatch(typeof(GenPlace), "TryPlaceDirect")]
	class Patches_TryPlaceDirect
	{
		[HarmonyPriority(Priority.Last)]
		static bool Prefix(ref bool __result, Thing thing, IntVec3 loc, Map map, Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			int? extraSlots = loc.GetFirstThingWithComp<CompExtraSlots>(map)?.GetComp<CompExtraSlots>().currentExtraSlots;
			if (extraSlots != null)
			{
				__result = true;
				Thing originalThing = thing;
				bool splitOff = false;
				if (thing.stackCount > thing.def.stackLimit)
				{
					thing = thing.SplitOff(thing.def.stackLimit);
					splitOff = true;
				}
				if (thing.def.stackLimit > 1)
				{
					foreach (var cellThing in loc.GetThingList(map))
					{
						if (!cellThing.CanStackWith(thing))
						{
							continue;
						}
						else
						{
							int stackCount = thing.stackCount;
							if (cellThing.TryAbsorbStack(thing, true))
							{
								resultingThing = cellThing;
								placedAction?.Invoke(cellThing, stackCount);
								//__result = !splitOff;
								return false;
							}
							resultingThing = null;
							if (stackCount != thing.stackCount)
							{
								placedAction?.Invoke(cellThing, stackCount - thing.stackCount);
							}
							if (originalThing != thing)
							{
								originalThing.TryAbsorbStack(thing, false);
								//__result = false;
								return false;
							}
						}
					}
				}
				resultingThing = GenSpawn.Spawn(thing, loc, map, WipeMode.Vanish);
				placedAction?.Invoke(thing, thing.stackCount);
				return false;
			}
			return true;
		}
	}
}

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
	[HarmonyPatch(typeof(GenDrop), nameof(GenDrop.TryDropSpawn))]
	class Patches_GenDrop
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return instructions.MethodReplacer(
				AccessTools.GetDeclaredMethods(typeof(GenPlace)).FindAll(x => x.Name == "TryPlaceThing")[1],
				AccessTools.Method(typeof(Patches_GenDrop), nameof(Patches_GenDrop.TryPlaceThing)));
		}

		static bool TryPlaceThing(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
		{
			int? extraSlots = dropCell.GetFirstThingWithComp<CompExtraSlots>(map)?.GetComp<CompExtraSlots>().currentExtraSlots;
			if (extraSlots != null)
			{
				Thing originalThing = thing;
				bool flag = true;
				if (thing.stackCount > thing.def.stackLimit)
				{
					thing = thing.SplitOff(thing.def.stackLimit);
					flag = false;
				}
				if (thing.def.stackLimit > 1)
				{
					foreach (Thing cellThing in dropCell.GetThingList(map))
					{
						if (cellThing.CanStackWith(thing))
						{
							int stackCount = thing.stackCount;
							if (cellThing.TryAbsorbStack(thing, true))
							{
								resultingThing = cellThing;
								placedAction?.Invoke(cellThing, stackCount);
								return flag;
							}
							resultingThing = null;
							if (placedAction != null && stackCount != thing.stackCount)
							{
								placedAction(cellThing, stackCount - thing.stackCount);
							}
							if (originalThing != thing)
							{
								if (originalThing.TryAbsorbStack(thing, false))
								{
									return true;
								}
							}
						}
					}
				}
				resultingThing = GenSpawn.Spawn(thing, dropCell, map, WipeMode.Vanish);
				placedAction?.Invoke(thing, thing.stackCount);
				return flag;
			}
			return GenPlace.TryPlaceThing(thing, dropCell, map, mode, out resultingThing, placedAction, nearPlaceValidator);
		}
	}
}

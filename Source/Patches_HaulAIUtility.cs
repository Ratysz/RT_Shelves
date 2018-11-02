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
	[HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToCellStorageJob))]
	class Patch_HaulToCellStorageJob
	{
		static void Postfix(ref Job __result, Pawn p, Thing t, IntVec3 storeCell)
		{
			if (t.Position == storeCell) // TODO verify.
			{
				return;
			}
			int? extraSlots = storeCell.GetFirstThingWithComp<CompExtraSlots>(p.Map)?.GetComp<CompExtraSlots>().currentExtraSlots;
			if (extraSlots != null)
			{
				int storedThings = 0;
				foreach (Thing thing in storeCell.GetThingList(p.Map))
				{
					if (thing.def.EverStorable(false))
					{
						if (thing.CanStackWith(t))
						{
							if (thing.stackCount >= thing.def.stackLimit)
							{
								storedThings++;
							}
							else
							{
								__result.count = t.def.stackLimit - thing.stackCount;
								return;
							}
						}
						else
						{
							storedThings++;
						}
					}
					if (storedThings > extraSlots)
					{
						return;
					}
				}
				__result.count = t.def.stackLimit;
			}
		}
	}
}

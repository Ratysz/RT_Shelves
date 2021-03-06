﻿using System;
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
	[HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
	class Patch_NoStorageBlockersIn
	{
		[HarmonyPriority(Priority.Last)]
		static bool Prefix(ref bool __result, IntVec3 c, Map map, Thing thing)
		{
			__result = false;
			int storedThings = 0;
			int? extraSlots = null;
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (extraSlots == null)
				{
					extraSlots = thing2.TryGetComp<CompExtraSlots>()?.currentExtraSlots;
				}
				if (thing2.def.EverStorable(false))
				{
					if (!thing2.CanStackWith(thing) || thing2.stackCount >= thing2.def.stackLimit)
					{
						storedThings++;
					}
				}
				if (thing2.def.entityDefToBuild != null && thing2.def.entityDefToBuild.passability != Traversability.Standable)
				{
					return false;
				}
				if (thing2.def.surfaceType == SurfaceType.None && thing2.def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			if ((extraSlots == null && storedThings > 0) || (extraSlots != null && storedThings > extraSlots))
			{
				return false;
			}
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
	internal class Patch_NoStorageBlockersIn_RSACompat
	{
		static MethodInfo RSASSMGetter;
		static FieldInfo RSASSMFPField;

		static bool Prepare(HarmonyInstance instance)
		{
			var rsaAsm = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(x => x.FullName.Split(',').First() == "RSA");
			if (rsaAsm != null)
			{
				Utility.Debug("Rimworld Search Agency detected, applying compatibility repatch.");

				var method = AccessTools.Method(typeof(StoreUtility), "NoStorageBlockersIn");
				var oldPatch = AccessTools.Method(rsaAsm.GetType("RSA.StoreUtility_NoStorageBlockersIn"), "FilledEnough");
				RSASSMGetter = AccessTools.Method(rsaAsm.GetType("RSA.HaulingHysterisis.StorageSettings_Mapping"), "Get");
				RSASSMFPField = AccessTools.Field(rsaAsm.GetType("RSA.HaulingHysterisis.StorageSettings_Hysteresis"), "FillPercent");

				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Utility.Debug("Removing RSA postfix to StoreUtility.NoStorageBlockersIn().");
					instance.Unpatch(method, oldPatch);
				});
				return true;
			}
			return false;
		}

		static float GetHysteresisFillPercent(StorageSettings settings)
		{
			return (float)RSASSMFPField.GetValue(RSASSMGetter.Invoke(null, new object[] { settings }));
		}

		static void Postfix(ref bool __result, IntVec3 c, Map map, Thing thing)
		{
			if (__result)
			{
				int? extraSlots = c.GetFirstThingWithComp<CompExtraSlots>(map)?.GetComp<CompExtraSlots>().currentExtraSlots;
				if (extraSlots == 0)
				{
					float pct = 100f;
					var settings = c.GetSlotGroup(map)?.Settings;
					if (settings != null)
					{
						pct = GetHysteresisFillPercent(settings);
					}
					__result &= !map.thingGrid.ThingsListAt(c).Any(t => t.def.EverStorable(false) && t.stackCount >= thing.def.stackLimit * (pct / 100f));
				}
			}
		}
	}

	[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.IsInValidBestStorage))]
	class Patch_IsInValidBestStorage
	{
		static void Postfix(ref bool __result, Thing t)
		{
			if (__result)
			{
				int? extraSlots = t.Position.GetFirstThingWithComp<CompExtraSlots>(t.Map)?.GetComp<CompExtraSlots>().currentExtraSlots;
				if (extraSlots != null && 1 + extraSlots < t.Position.GetThingList(t.Map).FindAll(x => x.def.EverStorable(false)).Count)
				{
					__result = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(StoreUtility), nameof(StoreUtility.TryFindBestBetterStoreCellFor))]
	class Patch_TryFindBestBetterStoreCellFor
	{
		static MethodInfo shouldHaulAway = AccessTools.Method(typeof(Patch_TryFindBestBetterStoreCellFor), nameof(Patch_TryFindBestBetterStoreCellFor.ShouldHaulAway));
		static MethodInfo markerMethod = AccessTools.Method(typeof(List<SlotGroup>), "get_Item");

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			int state = 0;
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Call, shouldHaulAway);
			yield return new CodeInstruction(OpCodes.Stloc_S, 8);
			foreach (var instruction in instructions)
			{
				yield return instruction;
				switch (state)
				{
					case 0:
						if (instruction.opcode == OpCodes.Callvirt && instruction.operand == markerMethod)
						{
							state++;
						}
						break;
					case 1:
						yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
						yield return new CodeInstruction(OpCodes.Brtrue, instructions.ToList().Find(x => x.operand?.GetHashCode() == 4).operand);
						state++;
						break;
					default:
						break;
				}
			}
		}

		static void Postfix()
		{
			// Fuck knows why, but this is necessary for the transpiler to work.
		}

		static bool ShouldHaulAway(Thing thing)
		{
			if (thing.Map != null)
			{
				int? extraSlots = thing.Position.GetFirstThingWithComp<CompExtraSlots>(thing.Map)?.GetComp<CompExtraSlots>().currentExtraSlots;
				if (extraSlots != null && 1 + extraSlots < thing.Position.GetThingList(thing.Map).FindAll(x => x.def.EverStorable(false)).Count)
				{
					return true;
				}
			}
			return false;
		}
	}
}

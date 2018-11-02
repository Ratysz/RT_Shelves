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
using UnityEngine;

namespace RT_Shelves
{
	public static class Utility
	{
		public static void Debug(string message)
		{
#if DEBUG
			Log.Message("[RT Shelves]: " + message, true);
#endif
		}

		public static CodeInstruction Inspect(this CodeInstruction instr, int? state = null)
		{
			Print(instr, state);
			return instr;
		}

		public static void Print(CodeInstruction instr, int? state = null)
		{
			if (instr.labels.Count > 0)
			{
				var labels = instr.labels.ToArray();
				StringBuilder builder = new StringBuilder();
				builder.Append($"LABELS : : : > {labels[0].GetHashCode()}");
				int index = 1;
				while (index < instr.labels.Count)
				{
					builder.Append($", {labels[index].GetHashCode()}");
					index++;
				}
				Debug(builder.ToString());
			}
			Debug($"INSTR : {(state != null ? ($"{state,-2}  ") : (""))}{instr.opcode,-10}\t : "
				+ ((instr.operand != null && instr.operand.GetType() == typeof(Label))
					? $": : > {instr.operand.GetHashCode(),-100}" : $"{instr.operand,-100}"));
		}
	}

	/*[HarmonyPatch(typeof(MouseoverReadout), nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
	class Patch_MouseoverReadoutOnGUI
	{
		public static Thing LastSelectedThing = null;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//MethodInfo markerMethod = AccessTools.Property(typeof(Color), nameof(Color.white)).GetGetMethod();
			MethodInfo markerMethod = AccessTools.Property(typeof(GUI), nameof(GUI.color)).GetSetMethod();
			MethodInfo sneakyMethod = AccessTools.Method(typeof(Patch_MouseoverReadoutOnGUI), nameof(Patch_MouseoverReadoutOnGUI.ExtraReadouts));
			int patchState = 0;
			foreach (var instruction in instructions)
			{
				if (patchState == 0 || patchState == 1)
				{
					if (instruction.opcode == OpCodes.Call && instruction.operand == markerMethod)
					{
						patchState++;
					}
				}
				else if (patchState == 2)
				{
					if (instruction.opcode == OpCodes.Call && instruction.operand == markerMethod)
					{
						yield return new CodeInstruction(OpCodes.Ldloc_1);
						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Call, sneakyMethod);
						patchState++;
					}
				}
				yield return instruction;
			}
		}

		static void ExtraReadouts(float num, Rect rect, IntVec3 cell)
		{
			if (LastSelectedThing != null)
			{
				rect = new Rect(15f, (float)UI.screenHeight - 65f - num, 999f, 999f);
				Widgets.Label(rect, " " + StoreUtility.IsGoodStoreCell(cell, Find.CurrentMap, LastSelectedThing, null, Faction.OfPlayer));
				num += 19f;
			}
		}
	}

	[HarmonyPatch]
	class Patch_SingleSelectedThing
	{
		static MethodInfo TargetMethod()
		{
			return AccessTools.Property(typeof(Selector), nameof(Selector.SingleSelectedThing)).GetGetMethod();
		}

		static void Postfix(Thing __result)
		{
			if (__result != null)
			{
				Patch_MouseoverReadoutOnGUI.LastSelectedThing = __result;
			}
		}
	}*/
}

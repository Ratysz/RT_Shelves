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
	[HarmonyPatch]
	class Patch_Spawn
	{
		static MethodInfo TargetMethod()
		{
			return AccessTools.GetDeclaredMethods(typeof(GenSpawn)).FindAll(x => x.Name == "Spawn")[2];
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo markerMethod = AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.WipeAndRefundExistingThings));
			int i = 0;
			int patchState = 0;
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (patchState == 0)
				{
					i++;
					if (instruction.opcode == OpCodes.Call && instruction.operand == markerMethod)
					{
						patchState++;
					}
				}
				else if (patchState == 1)
				{
					yield return new CodeInstruction(OpCodes.Pop);
					yield return new CodeInstruction(OpCodes.Br, instructions.ToList()[i + 4].operand);
					patchState++;
				}
			}
		}
	}
}

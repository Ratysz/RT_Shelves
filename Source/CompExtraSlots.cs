using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace RT_Shelves
{
	public class CompProperties_ExtraSlots : CompProperties
	{
		public int maxExtraSlots = 0;

		public CompProperties_ExtraSlots()
		{
			compClass = typeof(CompExtraSlots);
		}

		internal void ChIncrementMax(bool decrement = false)
		{
			maxExtraSlots = Math.Min(24, (Math.Max(0, maxExtraSlots + (decrement ? -1 : +1))));
		}

		internal void ChAssignMax(int value)
		{
			maxExtraSlots = Math.Min(24, (Math.Max(0, value)));
		}
	}

	public class CompExtraSlots : ThingComp
	{
		public int currentExtraSlots;
		private bool initialized = false;

		public CompProperties_ExtraSlots Props
		{
			get
			{
				return (CompProperties_ExtraSlots)props;
			}
		}

		private ThingDef upgradeDef;
		private ThingDef downgradeDef;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			upgradeDef = DefDatabase<ThingDef>.GetNamed(parent.def.defName + "_upgrade");
			downgradeDef = DefDatabase<ThingDef>.GetNamed(parent.def.defName + "_downgrade");
			if (!initialized)
			{
				currentExtraSlots = 0;
				initialized = true;
			}
		}

		private void DestroyUpgradeBlueprintsAndFrames(Map map)
		{
			foreach (var thing in parent.Position.GetThingList(map).ToList())
			{
				if ((thing is Frame || thing is Blueprint)
					&& (thing.def.entityDefToBuild == upgradeDef || thing.def.entityDefToBuild == downgradeDef))
				{
					thing.Destroy(DestroyMode.Cancel);
				}
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			DestroyUpgradeBlueprintsAndFrames(previousMap);
			for (int i = 0; i < currentExtraSlots; i++)
			{
				GenLeaving.DoLeavingsFor(parent, previousMap, DestroyMode.Deconstruct);
			}
		}

		internal class NumberyGizmo : Command_Action
		{
			private static readonly Texture2D steelIcon = DefDatabase<ThingDef>.GetNamed("Steel").uiIcon;
			private static readonly Texture2D componentIcon = DefDatabase<ThingDef>.GetNamed("ComponentIndustrial").uiIcon;
			private static readonly Texture2D silverIcon = DefDatabase<ThingDef>.GetNamed("Silver").uiIcon;
			private static readonly Texture2D hatIcon = DefDatabase<ThingDef>.GetNamed("Apparel_CowboyHat").uiIcon;

			int number;

			public NumberyGizmo(int currentExtraSlots)
			{
				number = currentExtraSlots;
			}

			public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
			{
				var result = base.GizmoOnGUI(topLeft, maxWidth);
				var color = GUI.color;
				GUI.color = Color.white;
				var font = Text.Font;
				Text.Font = GameFont.Medium;
				var anchor = Text.Anchor;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
				Widgets.DrawTextureFitted(rect.LeftPart(0.6f).TopPart(0.6f), steelIcon, 0.9f);
				Widgets.DrawTextureFitted(rect.RightPart(0.6f).TopPart(0.6f), componentIcon, 0.9f);
				Widgets.DrawTextureFitted(rect.RightPart(0.6f).BottomPart(0.6f), silverIcon, 0.9f);
				Widgets.DrawTextureFitted(rect.LeftPart(0.6f).BottomPart(0.6f), hatIcon, 0.9f);
				Widgets.DrawTextureFitted(rect, BGTex, 0.4f);
				Widgets.Label(rect, number.ToString());
				GUI.color = color;
				Text.Font = font;
				Text.Anchor = anchor;
				return result;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Props.maxExtraSlots != 0)
			{
				yield return new NumberyGizmo(currentExtraSlots + 1)
				{
					defaultLabel = "CompExtraSlots_Gizmo_Label".Translate(),
					defaultDesc = "CompExtraSlots_Gizmo_Description".Translate(),
					iconDrawScale = 0f,
					action = delegate
					{
						List<FloatMenuOption> options = new List<FloatMenuOption>();
						for (int i = 0; i <= Props.maxExtraSlots; i++)
						{
							int extraSlotsNumber = i;
							options.Add(new FloatMenuOption((extraSlotsNumber + 1).ToString(), delegate
							{
								DestroyUpgradeBlueprintsAndFrames(parent.Map);
								bool downgrade = currentExtraSlots > extraSlotsNumber;
								if (DebugSettings.godMode)
								{
									currentExtraSlots = extraSlotsNumber;
								}
								else
								{
									for (int j = 0; j < Math.Abs(currentExtraSlots - extraSlotsNumber); j++)
									{
										GenConstruct.PlaceBlueprintForBuild((downgrade ? downgradeDef : upgradeDef),
											parent.Position, parent.Map, parent.Rotation, parent.Faction, (downgrade ? null : parent.Stuff));
									}
								}
							}));
						}
						Find.WindowStack.Add(new FloatMenu(options));
					}
				};
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref currentExtraSlots, "currentExtraSlots", 0);
			Scribe_Values.Look(ref initialized, "initialized", true);
		}
	}
}

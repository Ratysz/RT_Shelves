using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using Harmony;
using UnityEngine;

namespace RT_Shelves
{
	class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			var harmony = HarmonyInstance.Create("io.github.ratysz.rt_shelves");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public override string SettingsCategory()
		{
			return "RT Shelves";
		}

		private ModSettings _settings;
		public ModSettings Settings
		{
			get => _settings ?? (_settings = GetSettings<ModSettings>());
			set => _settings = value;
		}

		public class ModSettings : Verse.ModSettings
		{
			public override void ExposeData()
			{
				if (Patch_GenerateImpliedDefs_PreResolve.Complete || Scribe.mode == LoadSaveMode.Saving)
				{
					StringBuilder builder = new StringBuilder();
					builder.AppendLine("Settings:");
					var defList = DefDatabase<ThingDef>.AllDefs.ToList().FindAll(x => x.comps?.Find(t => t is CompProperties_ExtraSlots) != null);
					foreach (var def in defList)
					{
						var props = def.GetCompProperties<CompProperties_ExtraSlots>();
						Scribe_Values.Look(ref props.maxExtraSlots, def.defName);
						builder.AppendLine($"    {def.defName} max extra slots: {props.maxExtraSlots}");
					}
					Utility.Debug(builder.ToString());
				}
				base.ExposeData();
			}
		}

		private void Separator(float y, Rect rect)
		{
			Color color = GUI.color;
			GUI.color = new Color(0.3f, 0.3f, 0.3f);
			Widgets.DrawLineHorizontal(0f, y, rect.width);
			GUI.color = color;
		}

		private void Spinner(Rect rect, ref int value, ref string buffer, bool active)
		{
			if (active)
			{
				buffer = value.ToString();
				if (Widgets.ButtonText(rect.LeftPartPixels(Text.LineHeight), "<"))
				{
					value--;
					buffer = value.ToString();
				}
				rect = rect.RightPartPixels(rect.width - Text.LineHeight);
				Widgets.TextFieldNumeric(rect.LeftPartPixels(Text.LineHeight), ref value, ref buffer);
				rect = rect.RightPartPixels(rect.width - Text.LineHeight);
				if (Widgets.ButtonText(rect.LeftPartPixels(Text.LineHeight), ">"))
				{
					value++;
					buffer = value.ToString();
				}
			}
			else
			{
				rect = rect.RightPartPixels(rect.width - Text.LineHeight);
				Widgets.Label(rect.LeftPartPixels(Text.LineHeight), value.ToString());
			}
		}

		private Vector2 scrollPosition;
		private readonly float rowHeight = 24f;
		private List<ThingDef> selectedDefs = new List<ThingDef>();
		private int maxExtraSlotsTemp;
		private string maxExtraSlotsBuffer;

		public override void DoSettingsWindowContents(Rect inRect)
		{
			List<ThingDef> list = DefDatabase<ThingDef>.AllDefs.ToList().FindAll(x => x.comps?.Find(t => t is CompProperties_ExtraSlots) != null);
			if (!list.NullOrEmpty())
			{
				Rect listRect = inRect.LeftPart(0.35f);
				Rect listViewRect = new Rect(0f, 0f, listRect.width - GenUI.ScrollBarWidth, list.Count * rowHeight);
				float vPos = 0f;
				bool first = true;
				Widgets.BeginScrollView(listRect, ref scrollPosition, listViewRect);
				bool multipleSelection = selectedDefs.Count > 1;

				foreach (var def in list)
				{
					var props = def.GetCompProperties<CompProperties_ExtraSlots>();
					Rect rowRect = new Rect(0f, vPos, listViewRect.width - 1f, rowHeight - 3f);
					if (!first)
					{
						Separator(vPos - 2f, rowRect);
					}
					else
					{
						first = false;
					}
					Widgets.DrawHighlightIfMouseover(rowRect);
					Widgets.Label(rowRect.LeftPartPixels(rowRect.width - 3 * Text.LineHeight), def.LabelCap ?? def.defName);
					bool active = selectedDefs.Contains(def);
					if (active)
					{
						Widgets.DrawHighlightSelected(rowRect);
						if (Widgets.ButtonInvisible(rowRect.LeftPartPixels(rowRect.width - 3 * Text.LineHeight)))
						{
							if (Event.current.modifiers == EventModifiers.Shift)
							{
								selectedDefs.Remove(def);
							}
							else
							{
								selectedDefs.Clear();
								if (multipleSelection)
								{
									selectedDefs.Add(def);
								}
							}
						}
					}
					else
					{
						if (Widgets.ButtonInvisible(rowRect))
						{
							if (Event.current.modifiers == EventModifiers.Shift)
							{
								selectedDefs.Add(def);
							}
							else
							{
								selectedDefs.Clear();
								selectedDefs.Add(def);
							}
						}
					}
					maxExtraSlotsTemp = props.maxExtraSlots + 1;
					Spinner(rowRect.RightPartPixels(3 * Text.LineHeight), ref maxExtraSlotsTemp, ref maxExtraSlotsBuffer, !multipleSelection && active);
					props.ChAssignMax(maxExtraSlotsTemp - 1);
					vPos += rowHeight;
				}

				if (multipleSelection)
				{
					Rect rect = new Rect(0f, vPos, listViewRect.width - 1f, rowHeight - 3f).RightPartPixels(3 * Text.LineHeight);
					if (Widgets.ButtonText(rect.LeftHalf(), "<"))
					{
						foreach (var def in selectedDefs)
						{
							var props = def.GetCompProperties<CompProperties_ExtraSlots>();
							props.ChIncrementMax(true);
						}
					}
					if (Widgets.ButtonText(rect.RightHalf(), ">"))
					{
						foreach (var def in selectedDefs)
						{
							var props = def.GetCompProperties<CompProperties_ExtraSlots>();
							props.ChIncrementMax();
						}
					}
				}
				Widgets.EndScrollView();
			}
		}
	}
}

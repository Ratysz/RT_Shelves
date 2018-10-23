using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using Harmony;

namespace RT_Shelves
{
	internal static class DefGen
	{
		internal static readonly MethodInfo generateBlueprint = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewBlueprintDef_Thing");
		internal static readonly MethodInfo generateFrame = AccessTools.Method(typeof(ThingDefGenerator_Buildings), "NewFrameDef_Thing");

		internal static ThingDef Blueprint(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null)
		{
			return (ThingDef)generateBlueprint.Invoke(null, new object[] { def, false, null });
		}

		internal static ThingDef Frame(ThingDef def)
		{
			return (ThingDef)generateFrame.Invoke(null, new object[] { def });
		}

		internal static ThingDef BaseDef(ThingDef baseDef, bool isDowngrade = false)
		{
			return new ThingDef
			{
				defName = baseDef.defName + (isDowngrade ? "_downgrade" : "_upgrade"),
				label = baseDef.label + (isDowngrade ? "_downgrade" : "_upgrade"), // TODO
				modContentPack = baseDef.modContentPack,
				stuffCategories = baseDef.stuffCategories,
				size = baseDef.size,
				rotatable = baseDef.rotatable,
				drawerType = baseDef.drawerType,
				graphicData = baseDef.graphicData,
				costList = baseDef.costList,
				costStuffCount = baseDef.costStuffCount,
				stuffProps = baseDef.stuffProps,
				constructionSkillPrerequisite = baseDef.constructionSkillPrerequisite,
				designationCategory = baseDef.designationCategory,
				statBases = baseDef.statBases,
				comps = new List<CompProperties>
				{
					new CompProperties_UpgradeApplier
					{
						targetDefName = baseDef.defName,
						isDowngrade = isDowngrade,
					},
				},

				menuHidden = true,
				tradeability = Tradeability.None,
				thingClass = typeof(Building),
				category = ThingCategory.Building,
				tickerType = TickerType.Normal,
				leaveResourcesWhenKilled = false,
				neverMultiSelect = true,
				fillPercent = 0f,
				coversFloor = false,
				neverOverlapFloors = true,
				blockPlants = false,
				blockLight = false,
				blockWind = false,
				building = new BuildingProperties
				{
					isInert = true,
					canPlaceOverWall = true,
					isEdifice = false,
					canPlaceOverImpassablePlant = true,
					
				},
				altitudeLayer = AltitudeLayer.Blueprint,
				passability = Traversability.Standable,
				castEdgeShadows = false,
				terrainAffordanceNeeded = TerrainAffordanceDefOf.Light,
				staticSunShadowHeight = 0f,
				clearBuildingArea = false,
			};
		}
	}
}

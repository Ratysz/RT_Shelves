using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RT_Shelves
{
	public class CompProperties_UpgradeApplier : CompProperties
	{
		public string targetDefName;
		public bool isDowngrade = false;

		public CompProperties_UpgradeApplier()
		{
			compClass = typeof(CompUpgradeApplier);
		}
	}

	public class CompUpgradeApplier : ThingComp
	{
		public CompProperties_UpgradeApplier Props
		{
			get
			{
				return (CompProperties_UpgradeApplier)props;
			}
		}

		private ThingDef defToUpgrade;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			defToUpgrade = DefDatabase<ThingDef>.GetNamed(Props.targetDefName);
		}

		public override void PostDeSpawn(Map map)
		{
			int increment;
			var thingToUpgrade = parent.Position.GetFirstThing(map, defToUpgrade);
			if (Props.isDowngrade)
			{
				increment = -1;
				GenLeaving.DoLeavingsFor(thingToUpgrade, map, DestroyMode.Deconstruct);
			}
			else
			{
				increment = +1;
			}
			thingToUpgrade.TryGetComp<CompExtraSlots>().currentExtraSlots += increment;
		}

		public override void CompTick()
		{
			if (!parent.Destroyed)
			{
				parent.Destroy();
			}
		}
	}
}

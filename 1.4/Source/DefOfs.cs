using RimWorld;
using Verse;

namespace Thek_GuardingPawns
{
    [DefOf]
    public static class GuardingJobsDefOf
    {
        public static JobDef GuardingP_GuardSpot;
        public static JobDef GuardingP_GuardPath;
        public static JobDef GuardingP_GuardPawn;
        //public static JobDef GuardingP_GuardArea;
    }

    [DefOf]
    public static class GotoJobDefOf
    {
        public static JobDef GuardingP_Goto;
    }

    [DefOf]
    public static class PawnTableDefOf
    {
        public static PawnTableDef GuardingP_PawnTableDef_Guard;
    }

    [DefOf]
    public static class WorkTypeDefOf
    {
        public static WorkTypeDef GuardingP_GuardingWorkType;
    }

    [DefOf]
    public static class GuardSpotDefOf
    {
        public static ThingDef GuardingP_redSpot;
        public static ThingDef GuardingP_yellowSpot;
        public static ThingDef GuardingP_orangeSpot;
        public static ThingDef GuardingP_blueSpot;
        public static ThingDef GuardingP_purpleSpot;

        private static HashSet<ThingDef> thingDefOfs;
        public static HashSet<ThingDef> GetDefOfs()
        {
            if (thingDefOfs != null)
            {
                return thingDefOfs;
            }

            thingDefOfs = new HashSet<ThingDef>();


            foreach (FieldInfo field in typeof(GuardSpotDefOf).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                object fieldValue = field.GetValue(null);
                if (fieldValue is ThingDef thingDef)
                {
                    thingDefOfs.Add(thingDef);
                }
            }
            return thingDefOfs;
        }
    }

    [DefOf]
    public static class GuardPathDefOf
    {
        public static ThingDef GuardingP_redPatrol;
        public static ThingDef GuardingP_yellowPatrol;
        public static ThingDef GuardingP_orangePatrol;
        public static ThingDef GuardingP_greenPatrol;
        public static ThingDef GuardingP_bluePatrol;
        public static ThingDef GuardingP_purplePatrol;
    }
}

using Verse.AI;

namespace Thek_GuardingPawns
{
    public class WorkGiver_GuardPath : WorkGiver_Scanner
    {
        private Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
        private MapComponent_GuardingPawns guardAssignmentMapComp;


        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            CacheMapComponent(pawn);
            bool shouldSkip = true;
            foreach (Thing thing in pawn.MapHeld.listerThings.AllThings)
            {
                if (GuardSpotDefOf.GetDefOfs().Contains(thing.def))
                {
                    shouldSkip = false;
                    break;
                }
            }
            if (shouldSkip)
            {
                return true;
            }
            return false;
        }


        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CacheMapComponent(pawn);
            guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs guardJob);
            if (guardJob is GuardJobs_GuardPath && !pawn.Map.reservationManager.IsReservedByAnyoneOf(t, pawn.Faction))
            {
                GuardJobs_GuardPath spot;
                spot = guardJob as GuardJobs_GuardPath;
                switch (spot.PathColor)
                {
                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath:
                        if (t.def == GuardPathDefOf.GuardingP_redPatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_orangePath:
                        if (t.def == GuardPathDefOf.GuardingP_orangePatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_yellowPath:
                        if (t.def == GuardPathDefOf.GuardingP_yellowPatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_bluePath:
                        if (t.def == GuardPathDefOf.GuardingP_bluePatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_greenPath:
                        if (t.def == GuardPathDefOf.GuardingP_greenPatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_purplePath:
                        if (t.def == GuardPathDefOf.GuardingP_purplePatrol) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, t);
                        break;
                }
            }
            return null;
        }


        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            CacheMapComponent(pawn);
            return guardAssignmentMapComp.SpotsOnMap;
        }


        private void CacheMapComponent(Pawn pawn)
        {
            if (!MapCompCache.TryGetValue(pawn.MapHeld, out MapComponent_GuardingPawns value))
            {
                value = pawn.MapHeld.GetComponent<MapComponent_GuardingPawns>();
                MapCompCache.Add(pawn.MapHeld, value);
            }
            guardAssignmentMapComp = value;
        }
    }
}
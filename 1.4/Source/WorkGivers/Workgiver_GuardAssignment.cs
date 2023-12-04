using Verse.AI;

namespace Thek_GuardingPawns
{
    public class Workgiver_GuardAssignment : WorkGiver_Scanner
    {
        private Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
        private MapComponent_GuardingPawns guardAssignmentMapComp;
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            CacheMapComponent(pawn);
            var assignedJob = guardAssignmentMapComp.GuardJobs.TryGetValue(pawn);
            switch (assignedJob)
            {
                case GuardJobs_GuardSpot:
                    bool shouldSkip = true;
                    foreach (Thing thing in pawn.MapHeld.listerThings.AllThings)
                    {
                        if (GuardSpotDefOf.GetDefOfs().Contains(thing.def))
                        {
                            shouldSkip = false;
                            break;
                        }
                    }
                    if (shouldSkip == true)
                    {
                        return true;
                    }
                    break;


                case GuardJobs_GuardPath:

                    break;


                case GuardJobs_GuardPawn:
                    foreach (Pawn pawnInMap in pawn.MapHeld.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawnInMap == pawn || pawnInMap.Dead)
                        {
                            return true;
                        }
                    }
                    break;

                default:
                case null:
                    return true;
            }
            return false;
        }


        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CacheMapComponent(pawn);
            var assignedJob = guardAssignmentMapComp.GuardJobs.TryGetValue(pawn);
            Log.Warning("Outside Switch");
            switch (assignedJob)
            {
                case GuardJobs_GuardSpot:
                    Log.Warning("JobMaker reached!");
                    return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
            }
            return null;
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
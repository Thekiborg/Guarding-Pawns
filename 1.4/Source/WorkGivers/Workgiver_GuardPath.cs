using Verse.AI;

namespace Thek_GuardingPawns
{
    public class WorkGiver_GuardPath : WorkGiver
    {
        private readonly Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
        private MapComponent_GuardingPawns guardAssignmentMapComp;


        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            CacheMapComponent(pawn);
            return !guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob)
                || gJob is not GuardJobs_GuardPath;
        }

        public override Job NonScanJob(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath);
            job.locomotionUrgency = LocomotionUrgency.Walk;

            return job;
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
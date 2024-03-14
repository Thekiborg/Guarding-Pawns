using Verse.AI;

namespace Thek_GuardingPawns
{
    public class WorkGiver_GuardPawn : WorkGiver_Scanner
    {
        private readonly Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
        private MapComponent_GuardingPawns guardAssignmentMapComp;


        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            CacheMapComponent(pawn);
            guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob);
            GuardJobs_GuardPawn protectJob = gJob as GuardJobs_GuardPawn;

            return gJob == null
                || gJob is not GuardJobs_GuardPawn
                || pawn == protectJob.pawnToGuard;
        }


        public override Job NonScanJob(Pawn pawn)
        {
            guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob);
            GuardJobs_GuardPawn protectJob = gJob as GuardJobs_GuardPawn;
            Job job = JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPawn, protectJob.pawnToGuard);
            job.locomotionUrgency = LocomotionUrgency.Sprint;

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

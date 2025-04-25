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
            if (!guardAssignmentMapComp.GuardJobs.ContainsKey(pawn) || !guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob))
            {
                return true;
            }
            if (gJob is not GuardJobs_GuardPawn gPawn)
            {
                return true;
            }
            if (pawn == gPawn.pawnToGuard)
            {
                return true;
            }
            if (!gPawn.pawnToGuard.Awake())
            {
                return true;
            }

            return false;
        }


        public override Job NonScanJob(Pawn pawn)
        {
            guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob);
            if (gJob is GuardJobs_GuardPawn protectJob && protectJob.pawnToGuard is not null)
            {
				Job job = JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPawn, protectJob.pawnToGuard);
				job.locomotionUrgency = LocomotionUrgency.Jog;

				return job;
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

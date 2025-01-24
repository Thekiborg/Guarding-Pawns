using System.Linq;
using Verse;
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
            if (!guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs gJob))
            {
                return true;
            }
            if (gJob is not GuardJobs_GuardPath)
            {
                return true;
            }


            GuardJobs_GuardPath gPathJob = gJob as GuardJobs_GuardPath;
            switch (gPathJob.PathColor)
            {
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath:
                    if (guardAssignmentMapComp.RedPatrolsOnMap.Count == 0) return true;
                    break;
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_orangePath:
                    if (guardAssignmentMapComp.OrangePatrolsOnMap.Count == 0) return true;
                    break;
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_yellowPath:
                    if (guardAssignmentMapComp.YellowPatrolsOnMap.Count == 0) return true;
                    break;
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_greenPath:
                    if (guardAssignmentMapComp.GreenPatrolsOnMap.Count == 0) return true;
                    break;
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_bluePath:
                    if (guardAssignmentMapComp.BluePatrolsOnMap.Count == 0) return true;
                    break;
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_purplePath:
                    if (guardAssignmentMapComp.PurplePatrolsOnMap.Count == 0) return true;
                    break;
            }
            return false;
        }


        /*
        private static bool SpotsOutsideAllowedArea(SortedList<int, Thing> sList, Pawn pawn)
        {
            if (sList.Any(kvp => kvp.Value.IsForbidden(pawn)))
            {
                return true;
            }
            return false;
        }
        */


        public override Job NonScanJob(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath);
            job.locomotionUrgency = LocomotionUrgency.Jog;

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
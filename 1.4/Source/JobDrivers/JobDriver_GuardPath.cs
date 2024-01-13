using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPath : JobDriver
    {
        MapComponent_GuardingPawns mapComp;
        List<Thing> spotsList;
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            guard.AddPreInitAction(delegate
            {
                if (spotsList.NullOrEmpty())
                {
                    GetSpotsList();
                    DoPrevSpotDictionary();
                    Thing newDest = spotsList[0 + mapComp.previousPatrolSpotPassedByPawn[pawn]]; 
                    //pawn.pather.StartPath(newDest, PathEndMode.OnCell);
                    Job job = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, newDest);
                    job.locomotionUrgency = LocomotionUrgency.Amble;
                    pawn.jobs.StopAll();
                    pawn.jobs.StartJob(job);
                }
            });
            guard.AddFinishAction(delegate
            {
                if (mapComp.previousPatrolSpotPassedByPawn.TryGetValue(pawn, out int index) && index == spotsList.Count - 1)
                {
                    mapComp.previousPatrolSpotPassedByPawn[pawn] = 0;
                }
                else
                {
                    mapComp.previousPatrolSpotPassedByPawn[pawn] += 1;
                }
            });
            yield return guard;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private void DoPrevSpotDictionary()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            mapComp.previousPatrolSpotPassedByPawn.TryAdd(pawn, 0);
        }

        private void GetSpotsList()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            if (!spotsList.NullOrEmpty())
            {
                spotsList.Clear();
            }
            switch ((mapComp.GuardJobs[pawn] as GuardJobs_GuardPath).PathColor)
            {
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath:
                    spotsList = [.. mapComp.RedPatrolsOnMap];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_orangePath:
                    spotsList = [.. mapComp.OrangePatrolsOnMap];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_yellowPath:
                    spotsList = [.. mapComp.YellowPatrolsOnMap];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_greenPath:
                    spotsList = [.. mapComp.GreenPatrolsOnMap];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_bluePath:
                    spotsList = [.. mapComp.BluePatrolsOnMap];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_purplePath:
                    spotsList = [.. mapComp.PurplePatrolsOnMap];
                    break;

                default:
                    break;
            }
        }
    }
}

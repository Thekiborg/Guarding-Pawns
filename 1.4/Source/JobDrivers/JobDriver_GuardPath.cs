using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPath : JobDriver
    {
        MapComponent_GuardingPawns mapComp;
        List<Thing> spotsList;
        int loop;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            guard.AddPreInitAction(delegate
            {
                if (spotsList.NullOrEmpty())
                {
                    GetSpotsList();
                    if (loop % 2 == 1)
                    {
                        spotsList.Reverse();
                    }
                    loop++;
                }
                pawn.pather.StartPath(spotsList[0].Position, PathEndMode.OnCell);
                spotsList.RemoveAt(0);
            });
            yield return guard;
            yield return Toils_General.Wait(2);
            yield return Toils_Jump.Jump(guard);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private List<Thing> GetSpotsList()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            switch ((mapComp.GuardJobs[pawn] as GuardJobs_GuardPath).PathColor)
            {
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath:
                    return spotsList = [.. mapComp.RedPatrolsOnMap];

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_orangePath:
                    return spotsList = [.. mapComp.OrangePatrolsOnMap];

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_yellowPath:
                    return spotsList = [.. mapComp.YellowPatrolsOnMap];

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_greenPath:
                    return spotsList = [.. mapComp.GreenPatrolsOnMap];

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_bluePath:
                    return spotsList = [.. mapComp.BluePatrolsOnMap];

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_purplePath:
                    return spotsList = [.. mapComp.PurplePatrolsOnMap];

                default:
                    return null;
            }
        }
    }
}

using Verse.AI;

namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardSpot : GuardJobs, IExposable
    {
        public GuardJobs_GuardSpot()
        {
            SpotColor = PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_redSpot;
        }
        public PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor? SpotColor;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SpotColor, "SpotColor");
        }
        public override Job GuardJob(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, cell.GetEdifice(pawn.Map));
        }
    }
}

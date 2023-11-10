using Verse.AI;

namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardPawn : GuardJobs, IExposable
    {
        public GuardJobs_GuardPawn()
        {
            pawnToGuard = null;
        }
        public Pawn pawnToGuard;

        public override void ExposeData()
        {
            Scribe_References.Look(ref pawnToGuard, "pawnToGuard");
        }
        public override Job GuardJob(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPawn, cell.GetEdifice(pawn.Map));
        }
    }
}
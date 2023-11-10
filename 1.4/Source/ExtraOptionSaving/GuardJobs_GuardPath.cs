using Verse.AI;

namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardPath : GuardJobs, IExposable
    {
        public GuardJobs_GuardPath()
        {
            PathColor = PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath;
        }
        public PawnColumnWorker_SelectJobExtras.GuardPathGroupColor? PathColor;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref PathColor, "PathColor");
        }
        public override Job GuardJob(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardPath, cell.GetEdifice(pawn.Map));
        }
    }
}

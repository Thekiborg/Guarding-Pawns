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
    }
}
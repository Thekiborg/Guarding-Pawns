using Verse.AI;

namespace Thek_GuardingPawns
{
    public abstract class GuardJobs : IExposable
    {
        public GuardJobs()
        {
            pawn = null;
        }
        public Pawn pawn;

        public abstract Job GuardJob(Pawn pawn, IntVec3 cell, bool forced = false);
        public abstract void ExposeData();
    }
}

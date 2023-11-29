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
        public abstract void ExposeData();
    }
}

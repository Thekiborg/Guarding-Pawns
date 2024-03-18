namespace Thek_GuardingPawns
{
    public abstract class GuardJobs : IExposable
    {
        internal GuardJobs()
        {
            pawn = null;
        }
        internal Pawn pawn;
        public abstract void ExposeData();
    }
}

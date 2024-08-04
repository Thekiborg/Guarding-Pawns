namespace Thek_GuardingPawns
{
    internal abstract class GuardJobs : IExposable
    {
        internal GuardJobs()
        {
            pawn = null;
        }
        internal Pawn pawn;
        public abstract void ExposeData();
    }
}
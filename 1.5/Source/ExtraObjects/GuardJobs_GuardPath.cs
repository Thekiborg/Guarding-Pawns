namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardPath : GuardJobs, IExposable
    {
        internal GuardJobs_GuardPath()
        {
            PathColor = PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath;
            shouldLoop = false;
        }
        internal PawnColumnWorker_SelectJobExtras.GuardPathGroupColor? PathColor;
        internal bool shouldLoop;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref PathColor, "PathColor");
            Scribe_Values.Look(ref shouldLoop, "shouldLoop");
        }
    }
}
namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardPath : GuardJobs, IExposable
    {
        public GuardJobs_GuardPath()
        {
            PathColor = PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath;
            shouldLoop = false;
        }
        public PawnColumnWorker_SelectJobExtras.GuardPathGroupColor? PathColor;
        public bool shouldLoop;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref PathColor, "PathColor");
            Scribe_Values.Look(ref shouldLoop, "shouldLoop");
        }
    }
}

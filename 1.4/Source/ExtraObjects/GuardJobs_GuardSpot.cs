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
    }
}

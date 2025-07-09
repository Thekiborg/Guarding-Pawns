namespace Thek_GuardingPawns
{
    internal class GuardJobs_GuardSpot : GuardJobs, IExposable
    {
        internal GuardJobs_GuardSpot()
        {
            SpotColor = PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_redSpot;
        }
        internal PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor SpotColor;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SpotColor, "SpotColor");
        }
    }
}
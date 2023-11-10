namespace Thek_GuardingPawns
{
    public class MapComponent_WindowTabButtonSelection : MapComponent
    {
        public MapComponent_WindowTabButtonSelection(Map map) : base(map) { }

        public Dictionary<Pawn, GuardJobs> GuardJobs = new();
        private List<Pawn> PawnsList = new();
        private List<GuardJobs> GuardJobsList = new();


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref GuardJobs, "GuardingP_GuardJobsExtraOptions", LookMode.Reference, LookMode.Deep, ref PawnsList,ref GuardJobsList);
        }
    }
}

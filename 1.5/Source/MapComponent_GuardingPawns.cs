namespace Thek_GuardingPawns
{
    public class MapComponent_GuardingPawns : MapComponent
    {
        public MapComponent_GuardingPawns(Map map) : base(map) { }

        internal Dictionary<Pawn, GuardJobs> GuardJobs = new();
        private List<Pawn> PawnsList = new();
        private List<GuardJobs> GuardJobsList = new();

        internal List<Thing> StandingSpotsOnMap = new();
        internal List<Thing> PatrolSpotsOnMap = new();

        internal SortedList<int, Thing> RedPatrolsOnMap = new();
        internal SortedList<int, Thing> OrangePatrolsOnMap = new();
        internal SortedList<int, Thing> YellowPatrolsOnMap = new();
        internal SortedList<int, Thing> GreenPatrolsOnMap = new();
        internal SortedList<int, Thing> BluePatrolsOnMap = new();
        internal SortedList<int, Thing> PurplePatrolsOnMap = new();

        internal Dictionary<Pawn, PatrolOptions> previousPatrolSpotPassedByPawn = new();
        private List<Pawn> prevPatrolPawnsList = new();
        private List<PatrolOptions> prevPatrolOptionsList = new();

        internal static bool shouldRenderGuardingSpots = true;
        internal static bool shouldRenderPatrollingSpots = true;
        //internal static bool shouldRenderAreaSpots = false;
        //internal static bool shouldOverrideAllowedArea = false;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref GuardJobs, "GuardingP_GuardJobsExtraOptions", LookMode.Reference, LookMode.Deep, ref PawnsList, ref GuardJobsList);
            Scribe_Collections.Look(ref previousPatrolSpotPassedByPawn, "GuardingP_PreviousPatrolSpotPassedByPawn", LookMode.Reference, LookMode.Deep, ref prevPatrolPawnsList, ref prevPatrolOptionsList);
            Scribe_Values.Look(ref shouldRenderGuardingSpots, "GuardingP_ShouldRenderGuardingSpots", true);
            Scribe_Values.Look(ref shouldRenderPatrollingSpots, "GuardingP_ShouldRenderPatrollingSpots", true);
        }
    }
}
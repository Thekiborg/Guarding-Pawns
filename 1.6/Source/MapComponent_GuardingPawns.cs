namespace Thek_GuardingPawns
{
	public class MapComponent_GuardingPawns : MapComponent
	{
		public MapComponent_GuardingPawns(Map map) : base(map) { }

		internal Dictionary<Pawn, GuardJobs> GuardJobs = [];
		private List<Pawn> PawnsList = [];
		private List<GuardJobs> GuardJobsList = [];

		internal List<Thing> StandingSpotsOnMap = [];
		internal List<Thing> PatrolSpotsOnMap = [];

		internal SortedList<int, Thing> RedPatrolsOnMap = [];
		internal SortedList<int, Thing> OrangePatrolsOnMap = [];
		internal SortedList<int, Thing> YellowPatrolsOnMap = [];
		internal SortedList<int, Thing> GreenPatrolsOnMap = [];
		internal SortedList<int, Thing> BluePatrolsOnMap = [];
		internal SortedList<int, Thing> PurplePatrolsOnMap = [];

		internal Dictionary<Pawn, PatrolOptions> previousPatrolSpotPassedByPawn = [];
		private List<Pawn> prevPatrolPawnsList = [];
		private List<PatrolOptions> prevPatrolOptionsList = [];

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
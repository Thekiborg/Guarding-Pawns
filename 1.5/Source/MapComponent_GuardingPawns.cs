using System.IO;
using System.Linq;

namespace Thek_GuardingPawns
{
    public class MapComponent_GuardingPawns : MapComponent
    {
        public MapComponent_GuardingPawns(Map map) : base(map) { }

        public Dictionary<Pawn, GuardJobs> GuardJobs = new();
        private List<Pawn> PawnsList = new();
        private List<GuardJobs> GuardJobsList = new();

        public List<Thing> StandingSpotsOnMap = new();
        public List<Thing> PatrolSpotsOnMap = new();

        public SortedList<int, Thing> RedPatrolsOnMap = new();
        public SortedList<int, Thing> OrangePatrolsOnMap = new();
        public SortedList<int, Thing> YellowPatrolsOnMap = new();
        public SortedList<int, Thing> GreenPatrolsOnMap = new();
        public SortedList<int, Thing> BluePatrolsOnMap = new();
        public SortedList<int, Thing> PurplePatrolsOnMap = new();

        public Dictionary<Pawn, PatrolOptions> previousPatrolSpotPassedByPawn = new();
        private List<Pawn> prevPatrolPawnsList = new();
        private List<PatrolOptions> prevPatrolOptionsList = new();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref GuardJobs, "GuardingP_GuardJobsExtraOptions", LookMode.Reference, LookMode.Deep, ref PawnsList, ref GuardJobsList);
            Scribe_Collections.Look(ref previousPatrolSpotPassedByPawn, "GuardingP_PreviousPatrolSpotPassedByPawn", LookMode.Reference, LookMode.Deep, ref prevPatrolPawnsList, ref prevPatrolOptionsList);
        }
    }
}

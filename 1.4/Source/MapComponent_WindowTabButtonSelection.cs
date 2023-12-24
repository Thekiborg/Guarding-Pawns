using System.Linq;
using Verse;

namespace Thek_GuardingPawns
{
    public class MapComponent_GuardingPawns : MapComponent
    {
        public MapComponent_GuardingPawns(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (map.IsHashIntervalTick(250))
            {
                AllHostilePawnsSpawned = map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.Faction.HostileTo(Faction.OfPlayer));
            }
        }

        public Dictionary<Pawn, GuardJobs> GuardJobs = new();
        private List<Pawn> PawnsList = new();
        private List<GuardJobs> GuardJobsList = new();

        public List<Thing> SpotsOnMap = new();

        public List<Thing> RedPatrolsOnMap = new();
        public List<Thing> OrangePatrolsOnMap = new();
        public List<Thing> YellowPatrolsOnMap = new();
        public List<Thing> GreenPatrolsOnMap = new();
        public List<Thing> BluePatrolsOnMap = new();
        public List<Thing> PurplePatrolsOnMap = new();

        public IEnumerable<Pawn> AllHostilePawnsSpawned;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref GuardJobs, "GuardingP_GuardJobsExtraOptions", LookMode.Reference, LookMode.Deep, ref PawnsList, ref GuardJobsList);
            Scribe_Collections.Look(ref SpotsOnMap, "GuardingP_SpotsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref RedPatrolsOnMap, "GuardingP_RedPatrolsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref OrangePatrolsOnMap, "GuardingP_OrangePatrolsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref YellowPatrolsOnMap, "GuardingP_YellowPatrolsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref GreenPatrolsOnMap, "GuardingP_GreenPatrolsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref BluePatrolsOnMap, "GuardingP_BluePatrolsOnMap", LookMode.Reference);
            Scribe_Collections.Look(ref PurplePatrolsOnMap, "GuardingP_PurplePatrolsOnMap", LookMode.Reference);
        }
    }
}

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

        public IEnumerable<Pawn> AllHostilePawnsSpawned;

        public Dictionary<Pawn, HostilityResponseMode> hostilityMode = new();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref GuardJobs, "GuardingP_GuardJobsExtraOptions", LookMode.Reference, LookMode.Deep, ref PawnsList, ref GuardJobsList);
            Scribe_Collections.Look(ref SpotsOnMap, "GuardingP_SpotsOnMap", LookMode.Reference);
        }
    }
}

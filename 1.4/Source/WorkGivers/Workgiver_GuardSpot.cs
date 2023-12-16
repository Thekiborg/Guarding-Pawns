using System.Linq;
using Verse.AI;

namespace Thek_GuardingPawns
{
    public class Workgiver_GuardSpot : WorkGiver_Scanner
    {
        private Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
        private MapComponent_GuardingPawns guardAssignmentMapComp;


        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            CacheMapComponent(pawn);
            bool shouldSkip = true;
            foreach (Thing thing in pawn.MapHeld.listerThings.AllThings)
            {
                if (GuardSpotDefOf.GetDefOfs().Contains(thing.def))
                {
                    shouldSkip = false;
                    break;
                }
            }
            if (shouldSkip)
            {
                return true;
            }
            return false;
        }


        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CacheMapComponent(pawn);
            guardAssignmentMapComp.GuardJobs.TryGetValue(pawn, out GuardJobs guardJob);
            if (guardJob is GuardJobs_GuardSpot && !pawn.Map.reservationManager.IsReservedByAnyoneOf(t, pawn.Faction))
            {
                GuardJobs_GuardSpot spot;
                spot = guardJob as GuardJobs_GuardSpot;
                switch (spot.SpotColor)
                {
                    case PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_redSpot:
                        if (t.def == GuardSpotDefOf.GuardingP_redSpot) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_orangeSpot:
                        if (t.def == GuardSpotDefOf.GuardingP_orangeSpot) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_yellowSpot:
                        if (t.def == GuardSpotDefOf.GuardingP_yellowSpot) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_blueSpot:
                        if (t.def == GuardSpotDefOf.GuardingP_blueSpot) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
                        break;

                    case PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_purpleSpot:
                        if (t.def == GuardSpotDefOf.GuardingP_purpleSpot) return JobMaker.MakeJob(GuardingJobsDefOf.GuardingP_GuardSpot, t);
                        break;
                }
            }
            return null;
        }


        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            CacheMapComponent(pawn);
            return guardAssignmentMapComp.SpotsOnMap;
        }


        private void CacheMapComponent(Pawn pawn)
        {
            if (!MapCompCache.TryGetValue(pawn.MapHeld, out MapComponent_GuardingPawns value))
            {
                value = pawn.MapHeld.GetComponent<MapComponent_GuardingPawns>();
                MapCompCache.Add(pawn.MapHeld, value);
            }
            guardAssignmentMapComp = value;
        }
    }
}
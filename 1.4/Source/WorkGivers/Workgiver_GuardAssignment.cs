﻿using Verse.AI;

namespace Thek_GuardingPawns
{
    public class Workgiver_GuardAssignment : WorkGiver_Scanner
    {
        private Dictionary<Map, MapComponent_WindowTabButtonSelection> MapCompCache = new();
        private MapComponent_WindowTabButtonSelection guardAssignmentMapComp;
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            var assignedJob = guardAssignmentMapComp.GuardJobs.TryGetValue(pawn);
            if (guardAssignmentMapComp.GuardJobs.Keys != null)
            {
                switch (assignedJob)
                {
                    case GuardJobs_GuardSpot:
                        foreach (ThingDef thing in GuardSpotDefOf.GetDefOfs())
                        {
                            if (!pawn.Map.listerThings.ThingsOfDef(thing).Any())
                            {
                                return true;
                            }
                        }
                        break;


                    case GuardJobs_GuardPath:

                        break;


                    case GuardJobs_GuardPawn:
                        foreach (Pawn pawnInMap in pawn.Map.mapPawns.FreeColonistsSpawned)
                        {
                            if (pawnInMap == pawn || pawnInMap.Dead)
                            {
                                return true;
                            }
                        }
                        break;

                    default:
                    case null:
                        return true;
                }
            }
            return false;
        }


        public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            CacheMapComponent(pawn);
            GuardJobs assignedJob = guardAssignmentMapComp.GuardJobs.TryGetValue(pawn);
            return assignedJob.GuardJob(pawn, cell);
        }


        private void CacheMapComponent(Pawn pawn)
        {
            if (!MapCompCache.ContainsKey(pawn.MapHeld))
            {
                MapCompCache.Add(pawn.MapHeld, pawn.MapHeld.GetComponent<MapComponent_WindowTabButtonSelection>());
                guardAssignmentMapComp = MapCompCache[pawn.MapHeld];
            }
        }
    }
}
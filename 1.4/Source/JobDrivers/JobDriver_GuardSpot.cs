using System.Linq;
using Verse;
using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor spotColor;
        MapComponent_GuardingPawns mapComp;
        const int meleeDetectionRange = 50;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.ReserveSittableOrSpot(job.targetA.Cell, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            GetSelectedSpot();

            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.handlingFacing = true;
            guard.tickAction = delegate
            {
                GuardJobs_GuardSpot guardJobSpot = mapComp.GuardJobs.TryGetValue(pawn) as GuardJobs_GuardSpot;
                guard.FailOn(() => spotColor != guardJobSpot.SpotColor);

                if (pawn.IsHashIntervalTick(60))
                {
                    // GIVEN BY KARIM
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        if (pawn.TryStartAttack(enemyPawn))
                        {
                            Log.Message("Early return");
                            return;
                        }
                    }
                    // GIVEN BY KARIM
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        if (pawn.equipment.Primary.DestroyedOrNull() || pawn.equipment.Primary.def.IsMeleeWeapon)
                        {
                            if (pawn.Position.DistanceToSquared(enemyPawn.Position) <= meleeDetectionRange
                                && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map)
                                && (!enemyPawn.Downed || !enemyPawn.Dead))
                            {
                                guard.handlingFacing = false;

                                // GIVEN BY KARIM
                                var targetTile = enemyPawn.Position;
                                Log.Message("A");
                                if (enemyPawn.pather.curPath != null && enemyPawn.pather.curPath.NodesLeftCount > 10)
                                {
                                    var tile = enemyPawn.pather.curPath.Peek(Math.Min(enemyPawn.pather.curPath.NodesLeftCount, 10));
                                    if (pawn.CanReach(tile, PathEndMode.OnCell, Danger.Unspecified))
                                    {
                                        targetTile = tile;
                                        Log.Message("B");
                                    }
                                }
                                if (pawn.CanReach(targetTile, PathEndMode.OnCell, Danger.Unspecified))
                                {
                                    pawn.pather.StartPath(targetTile, PathEndMode.Touch);
                                    pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                                    break;
                                }
                                // GIVEN BY KARIM
                            }
                        }
                        else if (pawn.Position.DistanceToSquared(enemyPawn.Position) <= pawn.equipment.PrimaryEq?.PrimaryVerb?.verbProps.range
                            && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map)
                            && (!enemyPawn.Downed || !enemyPawn.Dead))
                        {
                            // GIVEN BY KARIM
                            var targetTile = enemyPawn.Position;
                            if (!pawn.TryStartAttack(enemyPawn))
                            {
                                Log.Message("A");
                                if (enemyPawn.pather.curPath != null && enemyPawn.pather.curPath.NodesLeftCount > 10)
                                {
                                    var tile = enemyPawn.pather.curPath.Peek(Math.Min(enemyPawn.pather.curPath.NodesLeftCount - 1, 10));
                                    if (pawn.CanReach(tile, PathEndMode.OnCell, Danger.Unspecified))
                                    {
                                        targetTile = tile;
                                        Log.Message("B");
                                    }
                                }
                                if (pawn.CanReach(targetTile, PathEndMode.OnCell, Danger.Unspecified))
                                {
                                    if (!(pawn.CurrentEffectiveVerb?.IsMeleeAttack ?? true))
                                    {
                                        CastPositionRequest request = new();
                                        request.caster = pawn;
                                        request.target = enemyPawn;
                                        request.locus = targetTile;
                                        request.verb = pawn.CurrentEffectiveVerb;
                                        request.wantCoverFromTarget = true;
                                        if (CastPositionFinder.TryFindCastPosition(request, out var dest) && pawn.CanReach(dest, PathEndMode.OnCell, Danger.Unspecified))
                                        {
                                            targetTile = dest;
                                            Log.Message("C");
                                        }
                                    }
                                    pawn.pather.StartPath(targetTile, PathEndMode.Touch);
                                    pawn.mindState.enemyTarget = enemyPawn;
                                    pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                                    break;
                                }
                            }
                            // GIVEN BY KARIM
                        }
                    }
                }
            };
            guard.defaultCompleteMode = ToilCompleteMode.Delay;
            guard.AddPreInitAction(delegate
            {
                if (!mapComp.hostilityMode.ContainsKey(pawn)) { mapComp.hostilityMode.TryAdd(pawn, pawn.playerSettings.hostilityResponse); }
                pawn.playerSettings.hostilityResponse = mapComp.hostilityMode.TryGetValue(pawn);
            });
            guard.AddPreTickAction(delegate
            {
                Building building = pawn.Position.GetFirstBuilding(pawn.Map);
                if (building != null && !pawn.pather.Moving)
                {
                    guard.handlingFacing = true;
                    pawn.Rotation = building.Rotation;
                }
            });
            guard.defaultDuration = 1000;
            yield return guard;
            yield return Wait(2);
        }

        public static Toil Wait(int ticks)
        {
            Toil toil = ToilMaker.MakeToil("Wait");
            toil.initAction = toil.actor.pather.StopDead;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = ticks;
            toil.handlingFacing = true;
            toil.tickAction = delegate
            {
                Building building = toil.actor.Position.GetFirstBuilding(toil.actor.Map);
                if (building != null) toil.actor.Rotation = building.Rotation;
            };
            return toil;
        }

        private void GetSelectedSpot()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            GuardJobs_GuardSpot guardJobSpot = mapComp.GuardJobs.TryGetValue(pawn) as GuardJobs_GuardSpot;
            spotColor = (PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor)guardJobSpot.SpotColor;
        }
    }
}

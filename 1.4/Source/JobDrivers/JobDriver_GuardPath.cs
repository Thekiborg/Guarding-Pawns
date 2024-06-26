﻿using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPath : JobDriver
    {
        MapComponent_GuardingPawns mapComp;
        List<Thing> spotsList;
        readonly TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.LOSBlockableByGas | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        private Thing target;
        private bool anyHostileEverFound;
        Verb Verb
        {
            get
            {
                return pawn.CurrentEffectiveVerb;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Handles attacking manually until the threat response kicks in
            Toil attackUntilNoEnemies = ToilMaker.MakeToil("MakeNewToils");
            attackUntilNoEnemies.AddPreInitAction(delegate
            {
                AttackUntilNoEnemies(attackUntilNoEnemies);
            });
            attackUntilNoEnemies.defaultCompleteMode = ToilCompleteMode.FinishedBusy;


            // Makes the pawn walk to cover
            Toil movePawn = ToilMaker.MakeToil("MakeNewToils");
            movePawn.AddPreInitAction(() =>
            {
                if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    TryAttackRanged(attackUntilNoEnemies);
                }
                else
                {
                    anyHostileEverFound = true;
                    TryReachMelee();
                }
            });
            movePawn.defaultCompleteMode = ToilCompleteMode.PatherArrival;


            // Handles the behavior of the guard job and scanning for enemies
            Toil behaviorAndScan = ToilMaker.MakeToil("MakeNewToils");
            behaviorAndScan.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            behaviorAndScan.preInitActions ??= new List<Action>();
            behaviorAndScan.preInitActions.Add(delegate
            {
                if (spotsList.NullOrEmpty())
                {
                    GetSpotsList();
                    DoPrevSpotDictionary();
                    var pawnInPreviousPatrolDict = mapComp.previousPatrolSpotPassedByPawn[pawn];

                    if (pawnInPreviousPatrolDict.index > spotsList.Count - 1 || pawnInPreviousPatrolDict.index < 0)
                    {
                        pawnInPreviousPatrolDict.index = 0;
                    }
                    Thing newDest = spotsList[0 + pawnInPreviousPatrolDict.index];

                    if (newDest == null) EndJobWith(JobCondition.ErroredPather);
                    pawn.pather.StartPath(newDest, PathEndMode.OnCell);
                }

                target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Verb.verbProps.range, locus: pawn.Position);
                if (target == null) return;

                if (pawn.equipment != null && pawn.equipment.Primary != null)
                {
                    anyHostileEverFound = true;
                    JumpToToil(movePawn);
                }
            });
            behaviorAndScan.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            behaviorAndScan.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 120))
                {
                    target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Math.Max(Verb.verbProps.range, 35f));
                    if (target == null) return;

                    if (pawn.equipment != null)
                    {
                        anyHostileEverFound = true;
                        JumpToToil(movePawn);
                    }
                }
            };
            behaviorAndScan.preInitActions.Add(delegate
            {
                GuardJobs_GuardPath gJob = mapComp.GuardJobs[pawn] as GuardJobs_GuardPath;
                var dictContainsPawn = mapComp.previousPatrolSpotPassedByPawn.TryGetValue(pawn, out PatrolOptions patrolOptions);
                if (dictContainsPawn && gJob.shouldLoop == false)
                {
                    if (patrolOptions.index == spotsList.Count - 1)
                    {
                        mapComp.previousPatrolSpotPassedByPawn[pawn].index = 0;
                    }
                    else
                    {
                        mapComp.previousPatrolSpotPassedByPawn[pawn].index += 1;
                    }
                }
                else if (dictContainsPawn && gJob.shouldLoop)
                {
                    if (patrolOptions.isBacktracking == false)
                    {
                        mapComp.previousPatrolSpotPassedByPawn[pawn].index += 1;
                        if (mapComp.previousPatrolSpotPassedByPawn[pawn].index == spotsList.Count - 1) patrolOptions.isBacktracking = true;
                    }
                    else
                    {
                        if (mapComp.previousPatrolSpotPassedByPawn[pawn].index == 0) patrolOptions.isBacktracking = false;
                        mapComp.previousPatrolSpotPassedByPawn[pawn].index -= 1;
                        if (mapComp.previousPatrolSpotPassedByPawn[pawn].index == 0) patrolOptions.isBacktracking = false;
                    }
                }
            });

            Toil waitJumpObj = Wait(pawn, Rand.Range(30, 160));

            yield return behaviorAndScan;
            yield return Toils_Jump.JumpIf(waitJumpObj, () =>
            {
                return !anyHostileEverFound;
            });
            yield return movePawn;
            yield return attackUntilNoEnemies;
            yield return waitJumpObj;
            yield return Wait(pawn, Rand.Range(30, 120));
            yield return Wait(pawn, Rand.Range(30, 120));
            yield return Wait(pawn, Rand.Range(30, 120));
        }


        private void AttackUntilNoEnemies(Toil thisToil)
        {
            if (target is Pawn tPawn)
            {
                #region target is a Pawn
                bool flag = (!tPawn.Dead || !tPawn.Downed)
                    && Verb.CanHitTargetFrom(pawn.Position, tPawn)
                    || GenSight.LineOfSightToThing(pawn.Position, tPawn, Map);

                bool flag2 = (!tPawn.Dead || !tPawn.Downed)
                    || !Verb.CanHitTargetFrom(pawn.Position, tPawn)
                    || GenSight.LineOfSightToThing(pawn.Position, tPawn, Map);

                if (flag)
                {
                    pawn.TryStartAttack(tPawn);
                }

                if (flag2)
                {
                    tPawn = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Verb.verbProps.range, locus: TargetA.Cell);
                    if (tPawn != null)
                    {
                        if (pawn.mindState != null)
                        {
                            pawn.mindState.enemyTarget = tPawn;
                            JumpToToil(thisToil);
                        }
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
                #endregion
            }
            else
            {
                #region target is a Thing
                bool flag = target != null
                    && !target.Destroyed
                    && Verb.CanHitTargetFrom(pawn.Position, target)
                    || GenSight.LineOfSightToThing(pawn.Position, target, Map);

                bool flag2 = target == null
                    || target.Destroyed
                    || !Verb.CanHitTargetFrom(pawn.Position, target)
                    || GenSight.LineOfSightToThing(pawn.Position, target, Map);

                if (flag)
                {
                    pawn.TryStartAttack(target);
                }

                if (flag2)
                {
                    target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Verb.verbProps.range, locus: TargetA.Cell);
                    if (target != null)
                    {
                        if (pawn.mindState != null)
                        {
                            pawn.mindState.enemyTarget = target;
                            JumpToToil(thisToil);
                        }
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
                #endregion
            }
        }


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }


        private void TryAttackRanged(Toil movePawnToCoverToil)
        {
            if (target is Pawn pawnTarget)
            {
                #region target is Pawn
                if (Verb.CanHitTarget(pawnTarget) && (!pawnTarget.Dead || !pawnTarget.Downed))
                {
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.enemyTarget = pawnTarget;
                    }

                    CastPositionRequest cast = new()
                    {
                        caster = pawn,
                        target = pawn.mindState.enemyTarget,
                        verb = Verb,
                        maxRangeFromTarget = Verb.verbProps.range,
                        locus = job.targetA.Cell,
                        maxRangeFromLocus = Verb.verbProps.range,
                        wantCoverFromTarget = true,
                        maxRegions = 50
                    };

                    if (!CastPositionFinder.TryFindCastPosition(cast, out var dest))
                    {
                        return;
                    }
                    if (dest == pawn.Position)
                    {
                        JumpToToil(movePawnToCoverToil);
                    }
                    else
                    {
                        pawn.pather.StartPath(dest, PathEndMode.OnCell);
                    }
                    bool hasCover = CoverUtility.CalculateOverallBlockChance(pawn, pawnTarget.Position, Map) > 0.01f;
                    bool reservedCell = pawn.Position.Standable(Map) && Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
                    bool canHitTarget = Verb.CanHitTarget(pawnTarget);
                    if (hasCover && reservedCell && canHitTarget)
                    {
                        JumpToToil(movePawnToCoverToil);
                    }
                }
                #endregion
            }
            else if (target is Thing)
            {
                #region target is Thing
                if (Verb.CanHitTarget(target) && !target.Destroyed)
                {
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.enemyTarget = target;
                    }

                    CastPositionRequest cast = new()
                    {
                        caster = pawn,
                        target = pawn.mindState.enemyTarget,
                        verb = Verb,
                        maxRangeFromTarget = Verb.verbProps.range,
                        locus = job.targetA.Cell,
                        maxRangeFromLocus = Verb.verbProps.range,
                        wantCoverFromTarget = true,
                        maxRegions = 50
                    };

                    if (!CastPositionFinder.TryFindCastPosition(cast, out var dest))
                    {
                        return;
                    }
                    if (dest == pawn.Position)
                    {
                        JumpToToil(movePawnToCoverToil);
                    }
                    else
                    {
                        pawn.pather.StartPath(dest, PathEndMode.OnCell);
                    }
                    bool hasCover = CoverUtility.CalculateOverallBlockChance(pawn, target.Position, Map) > 0.01f;
                    bool reservedCell = pawn.Position.Standable(Map) && Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
                    bool canHitTarget = Verb.CanHitTarget(target);
                    if (hasCover && reservedCell && canHitTarget)
                    {
                        JumpToToil(movePawnToCoverToil);
                    }
                }
                #endregion
            }
        }


        private void TryReachMelee()
        {
            if (target is Pawn pawnTarget)
            {
                #region target is a pawn
                float meleeDetectRange = Math.Max((pawnTarget.CurrentEffectiveVerb?.verbProps.range ?? 20f) + 1, 35f);
                if (pawnTarget.Position.DistanceToSquared(pawn.Position) <= meleeDetectRange * meleeDetectRange)
                {
                    if ((!pawnTarget.Dead || !pawnTarget.Downed) && GenSight.LineOfSightToThing(pawn.Position, pawnTarget, Map))
                    {
                        if (pawn.mindState != null)
                        {
                            pawn.mindState.enemyTarget = pawnTarget;
                        }

                        if (pawnTarget.pather.curPath != null && pawnTarget.pather.curPath.NodesLeftCount > 0)
                        {
                            IntVec3 targetTile = pawnTarget.pather.curPath.Peek(pawnTarget.pather.curPath.NodesLeftCount - (pawnTarget.pather.curPath.NodesLeftCount / 3));
                            if (pawn.CanReach(targetTile, PathEndMode.OnCell, Danger.Deadly))
                            {
                                pawn.pather.StartPath(targetTile, PathEndMode.OnCell);
                            }
                        }
                        else
                        {
                            if (pawn.CanReach(pawnTarget, PathEndMode.Touch, Danger.Deadly))
                            {
                                pawn.pather.StartPath(pawnTarget, PathEndMode.Touch);
                            }
                        }
                    }
                }
                #endregion
            }
            else if (target is Thing)
            {
                #region target is a thing
                float meleeDetectRange = 35f;
                if (target.Position.DistanceToSquared(pawn.Position) <= meleeDetectRange * meleeDetectRange)
                {
                    if (!target.Destroyed && GenSight.LineOfSightToThing(pawn.Position, target, Map))
                    {
                        if (pawn.mindState != null)
                        {
                            pawn.mindState.enemyTarget = target;
                        }

                        if (pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                        {
                            pawn.pather.StartPath(target, PathEndMode.Touch);
                        }
                    }
                }
                #endregion
            }
        }



        private void DoPrevSpotDictionary()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            GuardJobs_GuardPath gJob = mapComp.GuardJobs[pawn] as GuardJobs_GuardPath;
            mapComp.previousPatrolSpotPassedByPawn.TryAdd(pawn, new PatrolOptions() { index = 0, isBacktracking = gJob.shouldLoop }); ;
        }


        public static Toil Wait(Pawn pawn, int ticks)
        {
            Toil toil = ToilMaker.MakeToil("Wait");
            toil.initAction = pawn.pather.StopDead;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = ticks;
            toil.handlingFacing = true;
            toil.AddPreInitAction(delegate
            {
                toil.actor.Rotation = Rot4.Random;
            });
            return toil;
        }


        private void GetSpotsList()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
            switch ((mapComp.GuardJobs[pawn] as GuardJobs_GuardPath).PathColor)
            {
                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath:
                    spotsList = [.. mapComp.RedPatrolsOnMap.Values];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_orangePath:
                    spotsList = [.. mapComp.OrangePatrolsOnMap.Values];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_yellowPath:
                    spotsList = [.. mapComp.YellowPatrolsOnMap.Values];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_greenPath:
                    spotsList = [.. mapComp.GreenPatrolsOnMap.Values];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_bluePath:
                    spotsList = [.. mapComp.BluePatrolsOnMap.Values];
                    break;

                case PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_purplePath:
                    spotsList = [.. mapComp.PurplePatrolsOnMap.Values];
                    break;

                default:
                    break;
            }
        }
    }
}
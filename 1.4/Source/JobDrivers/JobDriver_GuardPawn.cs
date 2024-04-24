using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPawn : JobDriver
    {
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
            behaviorAndScan.defaultCompleteMode = ToilCompleteMode.Delay;
            behaviorAndScan.defaultDuration = 2500;
            behaviorAndScan.preInitActions ??= new List<Action>();
            behaviorAndScan.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            behaviorAndScan.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 160))
                {
                    if (!((TargetA.Pawn.Position - pawn.Position).LengthHorizontal <= 10f) || !pawn.Position.WithinRegions(TargetA.Pawn.Position, Map, 2, TraverseParms.For(pawn)))
                    {
                        if (!pawn.CanReach(TargetA.Pawn, PathEndMode.Touch, Danger.Unspecified) || TargetA.Pawn.IsForbidden(pawn))
                        {
                            EndJobWith(JobCondition.Incompletable);
                        }
                        else if (!pawn.pather.Moving || pawn.pather.Destination != TargetA.Pawn)
                        {

                            pawn.pather.StartPath(TargetA.Pawn, PathEndMode.Touch);
                        }
                    }

                    target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Math.Max(Verb.verbProps.range, 35f));
                    if (target == null) return;

                    if (pawn.equipment != null)
                    {
                        anyHostileEverFound = true;
                        JumpToToil(movePawn);
                    }
                }
            };

            Toil waitToil = Toils_General.Wait(2);

            yield return behaviorAndScan;
            yield return Toils_Jump.JumpIf(waitToil, () =>
            {
                return !anyHostileEverFound;
            });
            yield return movePawn;
            yield return attackUntilNoEnemies;
            yield return waitToil;
            yield return Toils_General.Wait(120);
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
                if (pawnTarget.Position.DistanceToSquared(TargetA.Pawn.Position) <= meleeDetectRange * meleeDetectRange)
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



        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor spotColor;
        MapComponent_GuardingPawns mapComp;
        readonly TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        private Thing target;
        private bool anyHostileEverFound;

        Verb Verb
        {
            get
            {
                return pawn.CurrentEffectiveVerb;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            GetSelectedSpot();


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
                    if (!pawn.kindDef.canMeleeAttack)
                    {
                        Log.Warning($"{pawn.LabelShort}'s pawnKind cannot do melee attacks, exiting jobDriver.");
                        return;
                    }
                    anyHostileEverFound = true;
                    TryReachMelee();
                }
            });
            movePawn.defaultCompleteMode = ToilCompleteMode.PatherArrival;


            // Handles the behavior of the guard job and scanning for enemies
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil behaviorAndScan = ToilMaker.MakeToil("MakeNewToils");
            behaviorAndScan.preInitActions ??= new List<Action>();
            behaviorAndScan.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            GuardJobs_GuardSpot guardJobSpot = mapComp.GuardJobs.TryGetValue(pawn) as GuardJobs_GuardSpot;
            behaviorAndScan.FailOn(() => spotColor != guardJobSpot.SpotColor);
            behaviorAndScan.defaultDuration = 4000;
            behaviorAndScan.defaultCompleteMode = ToilCompleteMode.Delay;
            behaviorAndScan.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 160))
                {
                    target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Math.Max(Verb.EffectiveRange, 35f));
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
                Building building = pawn.Position.GetFirstBuilding(pawn.Map);
                if (building != null && !pawn.pather.Moving)
                {
                    behaviorAndScan.handlingFacing = true;
                    pawn.Rotation = building.Rotation;
                }
            });
            Toil waitToil = Wait(pawn, 2);


            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return behaviorAndScan;
            yield return Toils_Jump.JumpIf(waitToil, () =>
            {
                return !anyHostileEverFound;
            });
            yield return movePawn;
            yield return attackUntilNoEnemies;
            yield return waitToil;
        }


        private void TryReachMelee()
        {
            if (target is Pawn pawnTarget)
            {
                #region target is a pawn
                float meleeDetectRange = Math.Max((pawnTarget.CurrentEffectiveVerb?.EffectiveRange ?? 20f) + 1, 35f);
                if (pawnTarget.Position.DistanceToSquared(pawn.Position) <= meleeDetectRange * meleeDetectRange)
                {
                    if (!pawnTarget.DeadOrDowned && GenSight.LineOfSightToThing(pawn.Position, pawnTarget, Map))
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


        private void TryAttackRanged(Toil movePawnToCoverToil)
        {
            if (target is Pawn pawnTarget)
            {
                #region target is Pawn
                if (Verb.CanHitTarget(pawnTarget) && !pawnTarget.DeadOrDowned)
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
                        maxRangeFromTarget = Verb.EffectiveRange,
                        locus = job.targetA.Cell,
                        maxRangeFromLocus = Verb.EffectiveRange,
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
                        maxRangeFromTarget = Verb.EffectiveRange,
                        locus = job.targetA.Cell,
                        maxRangeFromLocus = Verb.EffectiveRange,
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


        private void AttackUntilNoEnemies(Toil thisToil)
        {
            if (target is Pawn tPawn)
            {
                #region target is a Pawn
                bool flag = tPawn != null
                    && !tPawn.Downed
                    && Verb.CanHitTargetFrom(pawn.Position, tPawn)
                    || GenSight.LineOfSightToThing(pawn.Position, tPawn, Map);

                bool flag2 = tPawn == null
                    || tPawn.DeadOrDowned
                    || !Verb.CanHitTargetFrom(pawn.Position, tPawn)
                    || GenSight.LineOfSightToThing(pawn.Position, tPawn, Map);

                if (flag)
                {
                    pawn.TryStartAttack(tPawn);
                }

                if (flag2)
                {
                    tPawn = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Verb.EffectiveRange, locus: TargetA.Cell);
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
                    target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: Verb.EffectiveRange, locus: TargetA.Cell);
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


        public static Toil Wait(Pawn pawn, int ticks)
        {
            Toil toil = ToilMaker.MakeToil("Wait");
            toil.initAction = pawn.pather.StopDead;
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
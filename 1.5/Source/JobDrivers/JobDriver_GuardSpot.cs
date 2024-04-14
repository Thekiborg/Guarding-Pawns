using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor spotColor;
        MapComponent_GuardingPawns mapComp;
        readonly TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.ReserveSittableOrSpot(job.targetA.Cell, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            GetSelectedSpot();

            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.preInitActions ??= new List<Action>();
            guard.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            GuardJobs_GuardSpot guardJobSpot = mapComp.GuardJobs.TryGetValue(pawn) as GuardJobs_GuardSpot;
            guard.FailOn(() => spotColor != guardJobSpot.SpotColor);
            guard.defaultDuration = 4000;
            guard.defaultCompleteMode = ToilCompleteMode.Delay;
            guard.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 120))
                {
                    if (pawn.equipment != null && pawn.equipment.Primary != null)
                    {
                        if (pawn.equipment.Primary.def.IsRangedWeapon)
                        {
                            TryAttackranged();
                        }
                        // add else melee
                    }

                }
            };
            guard.preInitActions.Add(delegate
            {
                Building building = pawn.Position.GetFirstBuilding(pawn.Map);
                if (building != null && !pawn.pather.Moving)
                {
                    guard.handlingFacing = true;
                    pawn.Rotation = building.Rotation;
                }
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return guard;
            yield return Wait(pawn, 2);
            //yield return Toils_Jump.Jump(guard);
        }


        private void TryAttackranged()
        {
            Verb pawnVerb = pawn.CurrentEffectiveVerb;
            Thing target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, minDist: 0f, maxDist: pawnVerb.EffectiveRange);
            if (target == null) return;

            if (target is Pawn pawnTarget)
            {
                job.SetTarget(TargetIndex.B, pawnTarget);
                if (pawnVerb.CanHitTarget(pawnTarget) && !pawnTarget.DeadOrDowned)
                {
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.enemyTarget = pawnTarget;
                    }

                    CastPositionRequest cast = new();
                    cast.caster = pawn;
                    cast.target = pawn.mindState.enemyTarget;
                    cast.verb = pawnVerb;
                    cast.maxRangeFromTarget = pawnVerb.EffectiveRange;
                    cast.locus = job.targetA.Cell;
                    cast.maxRangeFromLocus = pawnVerb.EffectiveRange;
                    cast.wantCoverFromTarget = true;
                    cast.maxRegions = 50;

                    if (!CastPositionFinder.TryFindCastPosition(cast, out var dest))
                    {
                        return;
                    }
                    if (dest == pawn.Position)
                    {
                        //try attack manually
                    }
                    else
                    {
                        pawn.pather.StartPath(dest, PathEndMode.OnCell);
                    }
                    bool hasCover = CoverUtility.CalculateOverallBlockChance(pawn, pawnTarget.Position, Map) > 0.01f;
                    bool reservedCell = pawn.Position.Standable(Map) && Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
                    bool canHitTarget = pawnVerb.CanHitTarget(pawnTarget);
                    if (hasCover && reservedCell && canHitTarget)
                    {
                        // try attack manually
                    }
                }
            }
            /*
            else
            {
                Pawn enemyPawn = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, null, 0);

                if (enemyPawn != null)
                {
                    var meleeDetectionRange = Math.Max((enemyPawn.CurrentEffectiveVerb?.verbProps.range ?? 25) + 1, 50f);
                    var distSqr = enemyPawn.Position.DistanceToSquared(pawn.Position);

                    if (nearestDistSqr > distSqr && distSqr < meleeDetectionRange * meleeDetectionRange)
                    {
                        if (!enemyPawn.Downed && pawn.CanReach(enemyPawn.Position, PathEndMode.Touch, Danger.Deadly) && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map))
                        {
                            nearestEnemy = enemyPawn;
                            nearestDistSqr = distSqr;
                        }
                    }
                }
            }
            if (nearestEnemy != null)
            {
                if (verb.IsMeleeAttack)
                {
                    if (nearestDistSqr >= 122 && nearestEnemy.pather.curPath != null && nearestEnemy.pather.curPath.NodesLeftCount > 10)
                    {
                        var tile = nearestEnemy.pather.curPath.Peek(Math.Min(nearestEnemy.pather.curPath.NodesLeftCount, 10));
                        if (pawn.CanReach(tile, PathEndMode.OnCell, Danger.Unspecified))
                        {
                            Job job_move = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, tile);
                            pawn.jobs.StopAll();
                            pawn.jobs.StartJob(job_move);
                            if (pawn.mindState != null)
                            {
                                pawn.mindState.enemyTarget = nearestEnemy;
                            }
                            return;
                        }
                    }
                    Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, nearestEnemy);
                    pawn.jobs.StopAll();
                    pawn.jobs.StartJob(job);
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.enemyTarget = nearestEnemy;
                    }
                }
            }*/
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
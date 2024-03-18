using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPawn : JobDriver
    {
        private const int wanderRange = 4;
        private readonly Func<Pawn, IntVec3, IntVec3, bool> validator;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            guard.preInitActions ??= new List<Action>();
            guard.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            guard.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 120))
                {
                    TryAttackEnemyPawn();
                    IntVec3 wanderDestination = RCellFinder.RandomWanderDestFor(pawn, TargetA.Pawn.Position, wanderRange, validator, Danger.Unspecified);
                    while (!WanderUtility.InSameRoom(wanderDestination, TargetLocA, pawn.Map))
                    {
                        wanderDestination = RCellFinder.RandomWanderDestFor(pawn, TargetA.Pawn.Position, wanderRange, validator, Danger.Unspecified);
                    }
                    pawn.pather.StartPath(wanderDestination, PathEndMode.OnCell);
                }
            };
            guard.preInitActions.Add(TryAttackEnemyPawn);
            yield return guard;
            yield return Toils_General.Wait(120);
        }


        private void TryAttackEnemyPawn()
        {
            Verb verb = pawn.CurrentEffectiveVerb;
            if (verb is not { state: VerbState.Idle })
            {
                return;
            }
            Pawn nearestEnemy = null;
            float nearestDistSqr = 2500;
            if (!verb.IsMeleeAttack)
            {
                TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.LOSBlockableByGas | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
                float effectiveRange = verb.verbProps.range * verb.verbProps.range * 4;
                Pawn enemyPawn = (Pawn)AttackTargetFinder.BestAttackTarget(TargetA.Pawn, targetScanFlags, null, 0, effectiveRange);

                if (enemyPawn != null && verb.CanHitTarget(enemyPawn) && !enemyPawn.Downed)
                {
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.enemyTarget = enemyPawn;
                    }
                    CastPositionRequest request = new();
                    request.caster = pawn;
                    request.target = enemyPawn;
                    request.verb = pawn.CurrentEffectiveVerb;
                    request.wantCoverFromTarget = true;
                    if (CastPositionFinder.TryFindCastPosition(request, out var dest) && dest != pawn.Position)
                    {
                        Job job_move = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, dest);
                        pawn.jobs.StopAll();
                        pawn.jobs.StartJob(job_move);
                    }
                    else
                    {
                        Job job_shoot = JobMaker.MakeJob(JobDefOf.Wait_Combat, enemyPawn, 400);
                        pawn.jobs.StopAll();
                        pawn.jobs.StartJob(job_shoot);
                    }
                    return;
                }

            }
            else
            {
                TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.LOSBlockableByGas | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
                Pawn enemyPawn = (Pawn)AttackTargetFinder.BestAttackTarget(TargetA.Pawn, targetScanFlags, null, 0);

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
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, nearestEnemy);
                        pawn.jobs.StopAll();
                        pawn.jobs.StartJob(job);
                        if (pawn.mindState != null)
                        {
                            pawn.mindState.enemyTarget = nearestEnemy;
                        }
                    }
                }
            }
        }


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
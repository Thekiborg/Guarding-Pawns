using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPawn : JobDriver
    {
        private const int wanderRange = 4;
        private readonly Func<Pawn, IntVec3, IntVec3, bool> validator;
        MapComponent_GuardingPawns mapComp;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            GetMapComp();
            IntVec3 wanderDestination = RCellFinder.RandomWanderDestFor(pawn, TargetA.Pawn.Position, wanderRange, validator, Danger.Unspecified);

            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            guard.preInitActions ??= new List<Action>();
            guard.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            guard.preInitActions.Add(delegate
            {
                Verb verb = pawn.CurrentEffectiveVerb;
                if (verb is not { state: VerbState.Idle })
                {
                    return;
                }
                Pawn nearestEnemy = null;
                float nearestDistSqr = 1000000f;
                if (!verb.IsMeleeAttack)
                {
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        if (verb.CanHitTarget(enemyPawn) && !enemyPawn.Downed)
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
                    float effectiveRange = verb.verbProps.range * verb.verbProps.range * 4;
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        var distSqr = enemyPawn.Position.DistanceToSquared(pawn.Position);
                        if (nearestDistSqr > distSqr && distSqr < effectiveRange)
                        {
                            if (GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map))
                            {
                                nearestEnemy = enemyPawn;
                                nearestDistSqr = distSqr;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
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
                                Job job = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, tile);
                                pawn.jobs.StopAll();
                                pawn.jobs.StartJob(job);
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
                    else
                    {
                        CastPositionRequest request = new();
                        request.caster = pawn;
                        request.target = nearestEnemy;
                        request.verb = verb;
                        request.maxRangeFromTarget = 50;
                        request.wantCoverFromTarget = true;
                        if (CastPositionFinder.TryFindCastPosition(request, out var dest))
                        {
                            Job job = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, dest);
                            job.expiryInterval = 60;
                            pawn.jobs.StopAll();
                            pawn.jobs.StartJob(job);
                            if (pawn.mindState != null)
                            {
                                pawn.mindState.enemyTarget = nearestEnemy;
                            }
                        }
                        else
                        {
                            Job job = JobMaker.MakeJob(GotoJobDefOf.GuardingP_Goto, nearestEnemy);
                            job.expiryInterval = 60;
                            job.expireRequiresEnemiesNearby = true;
                            pawn.jobs.StopAll();
                            pawn.jobs.StartJob(job);
                            if (pawn.mindState != null)
                            {
                                pawn.mindState.enemyTarget = nearestEnemy;
                            }
                        }
                    }
                }
            });
            guard.preInitActions.Add(delegate
            {

                pawn.pather.StartPath(wanderDestination, PathEndMode.OnCell);

            });
            yield return guard;
            yield return Toils_General.Wait(120);
        }


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private void GetMapComp()
        {
            mapComp = pawn.Map.GetComponent<MapComponent_GuardingPawns>();
        }
    }
}


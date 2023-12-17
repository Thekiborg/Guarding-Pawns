using System.Linq;
using Verse;
using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor spotColor;
        MapComponent_GuardingPawns mapComp;

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
            guard.defaultCompleteMode = ToilCompleteMode.Instant;
            guard.AddPreInitAction(delegate
            {
                if (!mapComp.hostilityMode.ContainsKey(pawn)) { mapComp.hostilityMode.TryAdd(pawn, pawn.playerSettings.hostilityResponse); }
                pawn.playerSettings.hostilityResponse = mapComp.hostilityMode.TryGetValue(pawn);
            });
            guard.preInitActions.Add(delegate
            {
                Verb verb = pawn.CurrentEffectiveVerb;
                if (verb is not { state: VerbState.Idle })
                {
                    return;
                }
                Pawn  nearestEnemy   = null;
                float nearestDistSqr = 1000000f;
                if (!verb.IsMeleeAttack)
                {
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        if (verb.CanHitTarget(enemyPawn))
                        {
                            if (pawn.mindState != null)
                            {
                                pawn.mindState.enemyTarget = enemyPawn;
                            }
                            pawn.mindState.enemyTarget = enemyPawn;
                            CastPositionRequest request    = new ();
                            request.caster              = pawn;
                            request.target              = enemyPawn;
                            request.verb                = pawn.CurrentEffectiveVerb;
                            request.wantCoverFromTarget = true;
                            if (CastPositionFinder.TryFindCastPosition(request, out var dest) && dest != pawn.Position)
                            {
                                Job job_move = JobMaker.MakeJob(JobDefOf.Goto, dest);
                                job_move.expiryInterval              = 60;
                                job_move.expireRequiresEnemiesNearby = true;
                                pawn.jobs.StopAll();
                                pawn.jobs.StartJob(job_move);
                                Job job_shoot = JobMaker.MakeJob(JobDefOf.Wait_Combat, enemyPawn, 400);
                                pawn.jobs.jobQueue.EnqueueFirst(job_shoot);
                            }
                            else
                            {
                                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                                Job job_shoot = JobMaker.MakeJob(JobDefOf.Wait_Combat, enemyPawn, 400);
                                pawn.jobs.StopAll();
                                pawn.jobs.StartJob(job_shoot);
                                pawn.mindState.enemyTarget = enemyPawn;
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
                                nearestEnemy   = enemyPawn;
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
                        var distSqr             = enemyPawn.Position.DistanceToSquared(pawn.Position);
                        if (nearestDistSqr > distSqr && distSqr < meleeDetectionRange * meleeDetectionRange)
                        {
                            if (pawn.CanReach(enemyPawn.Position, PathEndMode.Touch, Danger.Deadly) && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map))
                            {
                                nearestEnemy   = enemyPawn;
                                nearestDistSqr = distSqr;
                            }
                        }
                    }
                }
                if (nearestEnemy != null)
                {
                    pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                    if (verb.IsMeleeAttack)
                    {
                        Job     job;
                        if (nearestDistSqr >= 122 && nearestEnemy.pather.curPath != null && nearestEnemy.pather.curPath.NodesLeftCount > 10)
                        {
                            var tile = nearestEnemy.pather.curPath.Peek(Math.Min(nearestEnemy.pather.curPath.NodesLeftCount, 10));
                            if (pawn.CanReach(tile, PathEndMode.OnCell, Danger.Unspecified))
                            {
                                job = JobMaker.MakeJob(JobDefOf.Goto, tile);
                                pawn.jobs.StopAll();
                                pawn.jobs.StartJob(job);
                                pawn.mindState.enemyTarget = nearestEnemy;
                                return;
                            }
                        }
                        job = JobMaker.MakeJob(JobDefOf.Goto, nearestEnemy);
                        pawn.jobs.StopAll();
                        pawn.jobs.StartJob(job);
                    }
                    else
                    {
                        IntVec3             targetTile = pawn.Position;
                        CastPositionRequest request    = new ();
                        request.caster              = pawn;
                        request.target              = nearestEnemy;
                        request.verb                = pawn.CurrentEffectiveVerb;
                        request.wantCoverFromTarget = true;
                        if (CastPositionFinder.TryFindCastPosition(request, out var dest) && pawn.CanReach(dest, PathEndMode.OnCell, Danger.Unspecified))
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.Goto, targetTile);
                            job.expiryInterval              = 60;
                            job.expireRequiresEnemiesNearby = true;
                            pawn.jobs.StopAll();
                            pawn.jobs.StartJob(job);
                            pawn.mindState.enemyTarget = nearestEnemy;
                        }
                        else
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.Goto, nearestEnemy);
                            job.expiryInterval              = 60;
                            job.expireRequiresEnemiesNearby = true;
                            pawn.jobs.StopAll();
                            pawn.jobs.StartJob(job);
                            pawn.mindState.enemyTarget = nearestEnemy;
                        }
                    }
                }
            });
            guard.AddPreInitAction(delegate
            {
                Building building = pawn.Position.GetFirstBuilding(pawn.Map);
                if (building != null && !pawn.pather.Moving)
                {
                    guard.handlingFacing = true;
                    pawn.Rotation = building.Rotation;
                }
            });
            yield return guard;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return Wait(pawn, 2);
            yield return Toils_Jump.Jump(guard);
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

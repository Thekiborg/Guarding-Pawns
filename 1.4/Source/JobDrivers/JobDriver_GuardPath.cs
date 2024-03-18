using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPath : JobDriver
    {
        MapComponent_GuardingPawns mapComp;
        List<Thing> spotsList;


        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            guard.preInitActions ??= new List<Action>();
            guard.preInitActions.Add(delegate
            {
                TryAttackEnemyPawn();
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

                    pawn.pather.StartPath(newDest, PathEndMode.OnCell);
                }
            });
            guard.preInitActions.Add(delegate
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            });
            guard.tickAction = () =>
            {
                if (Gen.IsHashIntervalTick(pawn, 120))
                {
                    TryAttackEnemyPawn();
                }
            };
            guard.preInitActions.Add(delegate
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
            yield return guard;
            yield return Wait(pawn, Rand.Range(30, 160));
            yield return Wait(pawn, Rand.Range(30, 160));
            TryAttackEnemyPawn();
            yield return Wait(pawn, Rand.Range(30, 160));
            yield return Wait(pawn, Rand.Range(30, 160));
        }


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
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
                Pawn enemyPawn = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, null, 0, effectiveRange);

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

using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor spotColor;
        MapComponent_GuardingPawns mapComp;
        const int meleeDetectionRange = 25;

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

                if (Gen.IsHashIntervalTick(pawn, 60))
                {
                    foreach (Pawn enemyPawn in Map.mapPawns.AllPawnsSpawned)
                    {
                        if (pawn.equipment.Primary.DestroyedOrNull() || pawn.equipment.Primary.def.IsMeleeWeapon)
                        {
                            if (pawn.Position.DistanceTo(enemyPawn.Position) <= meleeDetectionRange && enemyPawn.Faction.HostileTo(pawn.Faction) && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map) && !enemyPawn.Downed)
                            {
                                pawn.pather.StartPath(enemyPawn.Position, PathEndMode.Touch);
                                pawn.meleeVerbs.TryMeleeAttack(enemyPawn);
                            }
                        }
                        if (pawn.Position.DistanceTo(enemyPawn.Position) <= pawn.equipment.PrimaryEq?.PrimaryVerb?.verbProps.range && enemyPawn.Faction.HostileTo(pawn.Faction))
                        {
                            pawn.TryStartAttack(enemyPawn);
                            return;
                        }
                    }
                }

                Rot4 buildingRot = pawn.Position.GetFirstBuilding(pawn.Map).Rotation;
                pawn.Rotation = buildingRot;
            };
            guard.AddFinishAction(delegate
            {
                /*
                Aelanna — Today at 1:09 AM
                Well yes, because the Wait toil handles the facing
                But you have to have something to look at, you can't directly control the facing direction
                Or you can make your own custom wait toil, it's not like it's hidden code:
                */
            });
            guard.defaultCompleteMode = ToilCompleteMode.Delay;
            guard.defaultDuration = 1000;
            yield return guard;
            yield return Wait(100);
        }

        public static Toil Wait(int ticks)
        {
            Toil toil = ToilMaker.MakeToil("Wait");
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = ticks;
            toil.handlingFacing = true;
            toil.tickAction = delegate
            {
                Rot4 buildingRot = toil.actor.Position.GetFirstBuilding(toil.actor.Map).Rotation;
                toil.actor.Rotation = buildingRot;
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

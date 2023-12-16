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

                if (pawn.IsHashIntervalTick(60))
                {
                    foreach (Pawn enemyPawn in mapComp.AllHostilePawnsSpawned)
                    {
                        if (pawn.equipment.Primary.DestroyedOrNull() || pawn.equipment.Primary.def.IsMeleeWeapon)
                        {
                            if (pawn.Position.DistanceTo(enemyPawn.Position) <= meleeDetectionRange
                                && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map)
                                && !enemyPawn.Downed)
                            {
                                guard.handlingFacing = false;
                                pawn.pather.StartPath(enemyPawn.Position, PathEndMode.Touch);
                                if (pawn.Position.DistanceTo(enemyPawn.Position) <= 1) { pawn.meleeVerbs.TryMeleeAttack(enemyPawn); }
                            }
                        }
                        if (pawn.Position.DistanceToSquared(enemyPawn.Position) <= pawn.equipment.PrimaryEq?.PrimaryVerb?.verbProps.range
                            && GenSight.LineOfSightToThing(pawn.Position, enemyPawn, pawn.Map)
                            && !enemyPawn.Downed)
                        {
                            pawn.TryStartAttack(enemyPawn);
                        }
                    }
                }
            };
            guard.defaultCompleteMode = ToilCompleteMode.Delay;
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

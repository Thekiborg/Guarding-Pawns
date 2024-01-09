using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardPath : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil guard = ToilMaker.MakeToil("MakeNewToils");
            guard.defaultCompleteMode = ToilCompleteMode.Never;
            guard.tickAction = delegate
            {
                guard.actor.pather.StopDead();
            };
            yield return guard;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}

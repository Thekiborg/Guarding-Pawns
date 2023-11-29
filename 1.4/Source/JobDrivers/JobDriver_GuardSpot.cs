using Verse.AI;

namespace Thek_GuardingPawns
{
    public class JobDriver_GuardSpot : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.targetA, job))
            {
                return false;
            }
            if (!pawn.ReserveSittableOrSpot(job.targetA.Cell, job))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Error("MakeNewToils reached!");
            yield break;
        }
    }
}

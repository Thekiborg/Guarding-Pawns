namespace Thek_GuardingPawns
{
#pragma warning disable CS0649
	internal class PatrolOptions : IExposable
    {
        internal PatrolOptions()
        {
            index = 0;
            isBacktracking = false;
        }
        internal int index;
        internal bool isBacktracking;
        internal bool reachedSpot;

        public void ExposeData()
        {
            Scribe_Values.Look(ref index, "GuardingP_LastSpotIndex");
            Scribe_Values.Look(ref isBacktracking, "GuardingP_IsBacktracking");
        }
    }
}
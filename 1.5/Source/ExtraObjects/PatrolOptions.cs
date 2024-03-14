namespace Thek_GuardingPawns
{
    public class PatrolOptions : IExposable
    {
        public PatrolOptions() 
        {
            index = 0;
            isBacktracking = false;
        }
        public int index;
        public bool isBacktracking;

        public void ExposeData()
        {
            Scribe_Values.Look(ref index, "GuardingP_LastSpotIndex");
            Scribe_Values.Look(ref isBacktracking, "GuardingP_IsBacktracking");
        }
    }
}

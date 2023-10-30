namespace Thek_GuardingPawns
{
    public class GuardJobs_GuardPath(Pawn pawn, PawnColumnWorker_SelectJobExtras.GuardPathGroupColor PathColor) : GuardJobs(pawn)
    {
        public PawnColumnWorker_SelectJobExtras.GuardPathGroupColor PathColor = PathColor;
        
        private void GetData()
        {
            foreach (Pawn pawn in PawnColumnWorker_SelectJobExtras.PathColor.Keys)
            {
                // Here i get pawn as the pawn getting the option
                //And PathColor[pawn], the value for that pawn
            }
        }
    }
}

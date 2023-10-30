namespace Thek_GuardingPawns
{
    public class MapComponent_WindowTabButtonSelection : MapComponent
    {
        public MapComponent_WindowTabButtonSelection(Map map) : base(map) { }

        public Dictionary<Pawn, GuardJobs> GuardJobsExtraOptions = new();

        public Dictionary<Pawn, PawnColumnWorker_SelectJob.GuardJobType> JobButtonSelection = new();
        public List<Pawn> PawnsList = new();
        public List<PawnColumnWorker_SelectJob.GuardJobType> GuardJobTypes = new();

        public Dictionary<Pawn, PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor> GuardSpotColorButtonSelection = new();
        public List<Pawn> PawnsListExtraGuardSpot = new();
        public List<PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor> GuardSpotColors = new();

        public Dictionary<Pawn, PawnColumnWorker_SelectJobExtras.GuardPathGroupColor> GuardPathColorButtonSelection = new();
        public List<Pawn> PawnsListExtraGuardPath = new();
        public List<PawnColumnWorker_SelectJobExtras.GuardPathGroupColor> GuardPathColors = new();

        public Dictionary<Pawn, Pawn> GuardPawnButtonSelection = new();
        public List<Pawn> PawnsListExtraGuardPawn = new();
        public List<Pawn> PawnsToGuard = new();


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref JobButtonSelection, "GuardingP_JobSelections", LookMode.Reference, LookMode.Value, ref PawnsList,ref GuardJobTypes);
            Scribe_Collections.Look(ref GuardSpotColorButtonSelection, "GuardingP_GuardSpotColorSelection", LookMode.Reference, LookMode.Value, ref PawnsListExtraGuardSpot, ref GuardSpotColors);
            Scribe_Collections.Look(ref GuardPathColorButtonSelection, "GuardingP_GuardPathColorSelection", LookMode.Reference, LookMode.Value, ref PawnsListExtraGuardPath, ref GuardPathColors);
            Scribe_Collections.Look(ref GuardPawnButtonSelection, "GuardingP_GuardPawnButtonSelection", LookMode.Reference, LookMode.Reference, ref PawnsListExtraGuardPawn, ref PawnsToGuard);
            JobButtonSelection ??= new();
            GuardSpotColorButtonSelection ??= new();
            GuardPathColorButtonSelection ??= new();
            GuardPawnButtonSelection ??= new();
        }
    }
}

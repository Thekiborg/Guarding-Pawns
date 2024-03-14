using System.Linq;

namespace Thek_GuardingPawns
{
    public class MainTabWindow_Guards : MainTabWindow_PawnTable
    {
        const float padding = 5f + CheckboxPadding;
        const float CheckboxPadding = 16f;

        private Rect windowTabRect;
        private Rect firstRectLabel;
        private Rect secondRectLabel;

        public static bool shouldRenderGuardingSpots = true;
        public static bool shouldRenderPatrollingSpots = true;

        private static readonly float GuardingSpotKeyWidth = Text.CalcSize("GuardingP_GuardingSpotCheckBox").x;
        private static readonly float PatrollingSpotKeyWidth = Text.CalcSize("GuardingP_PatrollingSpotCheckBox").x;
        //private static readonly float GuardingAreaKeyWidth = Text.CalcSize("GuardingP_GuardingAreaCheckBox").x;

        //public static bool shouldRenderAreaSpots = false;

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.GuardingP_PawnTableDef_Guard;
        protected override IEnumerable<Pawn> Pawns => base.Pawns.Where((Pawn pawn) => pawn.IsFreeNonSlaveColonist && pawn.workSettings.WorkIsActive(WorkTypeDefOf.GuardingP_GuardingWorkType));
        

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);
            windowTabRect = rect; 
            bool prevGuardingValue = shouldRenderGuardingSpots;
            bool prevPatrollingValue = shouldRenderPatrollingSpots;

            Widgets.Checkbox((float)rect.xMax - Widgets.CheckboxSize - CheckboxPadding, rect.y, ref shouldRenderGuardingSpots);
            Widgets.Label(new Rect(FirstRectLabelUtility()), "GuardingP_GuardingSpotCheckBox".Translate());
            if (prevGuardingValue != shouldRenderGuardingSpots)
            {
                Map map = Find.CurrentMap;
                foreach (Thing spot in map.listerBuildings.allBuildingsColonist)
                {
                    if (GuardSpotDefOf.GetDefOfs().Contains(spot.def))
                    {
                        spot.DirtyMapMesh(map);
                    }
                }
            }

            Widgets.Checkbox((float)firstRectLabel.xMin - padding - Widgets.CheckboxSize, rect.y, ref shouldRenderPatrollingSpots);
            Widgets.Label(new Rect(SecondRectLabelUtility()), "GuardingP_PatrollingSpotCheckBox".Translate());
            if (prevPatrollingValue != shouldRenderGuardingSpots)
            {
                Map map = Find.CurrentMap;
                foreach (Thing spot in map.listerBuildings.allBuildingsColonist)
                {
                    if (GuardPathDefOf.GetDefOfs().Contains(spot.def))
                    {
                        spot.DirtyMapMesh(map);
                    }
                }
            }

            //Widgets.Checkbox((float)secondRectLabel.xMin - padding - Widgets.CheckboxSize, rect.y, ref shouldRenderAreaSpots);
            //Widgets.Label(new Rect(ThirdRectLabelUtility()), "GuardingP_GuardingAreaCheckBox".Translate());

        }


        private Rect FirstRectLabelUtility()
        {
            Rect FirstRectLabel = new(windowTabRect.xMax - GuardingSpotKeyWidth + Widgets.CheckboxSize + CheckboxPadding, windowTabRect.y, GuardingSpotKeyWidth, windowTabRect.height);
            firstRectLabel = FirstRectLabel;
            return FirstRectLabel;
        }

        private Rect SecondRectLabelUtility()
        {
            Rect SecondRectLabel = new(firstRectLabel.xMin - PatrollingSpotKeyWidth + Widgets.CheckboxSize + padding, windowTabRect.y, PatrollingSpotKeyWidth, windowTabRect.height);
            secondRectLabel = SecondRectLabel;
            return SecondRectLabel;
        }
        /*
        private Rect ThirdRectLabelUtility()
        {
            Rect ThirdRectLabel = new(secondRectLabel.xMin - GuardingAreaKeyWidth + Widgets.CheckboxSize + padding, windowTabRect.y, GuardingAreaKeyWidth, windowTabRect.height);
            return ThirdRectLabel;
        }
        */

        protected override float ExtraTopSpace
        {
            get
            {
                return 30f;
            }
        }
    }
}

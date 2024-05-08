using System.Linq;

namespace Thek_GuardingPawns
{
    public class MainTabWindow_Guards : MainTabWindow_PawnTable
    {
        const float padding = 5f + CheckboxPadding;
        const float CheckboxPadding = 8f;

        private Rect windowTabRect;
        private Rect firstRectLabel;
        private Rect secondRectLabel;
        //private Rect thirdRectLabel;
        //private Rect leftRectLabel;

        private static readonly float GuardingSpotKeyWidth = Text.CalcSize("GuardingP_GuardingSpotCheckBox".Translate()).x;
        private static readonly float PatrollingSpotKeyWidth = Text.CalcSize("GuardingP_PatrollingSpotCheckBox".Translate()).x;
        //private static readonly float GuardingAreaKeyWidth = Text.CalcSize("GuardingP_GuardingAreaCheckBox").x;
        //private static readonly float OverrideAllowedAreaWidth = Text.CalcSize("GuardingP_OverrideAllowedAreaCheckBox".Translate()).x;


        protected override float ExtraTopSpace
        {
            get
            {
                return 30f;
            }
        }

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.GuardingP_PawnTableDef_Guard;
        protected override IEnumerable<Pawn> Pawns => base.Pawns.Where((Pawn pawn) => (pawn.DevelopmentalStage != DevelopmentalStage.Newborn) && pawn.IsFreeNonSlaveColonist && pawn.workSettings.WorkIsActive(WorkTypeDefOf.GuardingP_GuardingWorkType));


        public override void DoWindowContents(Rect rect)
        {
            windowTabRect = rect;
            base.DoWindowContents(rect);
            bool prevGuardingValue = MapComponent_GuardingPawns.shouldRenderGuardingSpots;
            bool prevPatrollingValue = MapComponent_GuardingPawns.shouldRenderPatrollingSpots;
            Map map = Find.CurrentMap;

            Widgets.Checkbox(rect.xMax - Widgets.CheckboxSize - CheckboxPadding, rect.y, ref MapComponent_GuardingPawns.shouldRenderGuardingSpots);
            Widgets.Label(new Rect(FirstRectLabelUtility()), "GuardingP_GuardingSpotCheckBox".Translate());
            if (prevGuardingValue != MapComponent_GuardingPawns.shouldRenderGuardingSpots)
            {
                foreach (Thing spot in map.listerBuildings.allBuildingsColonist)
                {
                    if (GuardSpotDefOf.GetDefOfs().Contains(spot.def))
                    {
                        spot.DirtyMapMesh(map);
                    }
                }
            }

            Widgets.Checkbox(firstRectLabel.xMin - padding - Widgets.CheckboxSize - CheckboxPadding, rect.y, ref MapComponent_GuardingPawns.shouldRenderPatrollingSpots);
            Widgets.Label(new Rect(SecondRectLabelUtility()), "GuardingP_PatrollingSpotCheckBox".Translate());
            if (prevPatrollingValue != MapComponent_GuardingPawns.shouldRenderGuardingSpots)
            {

                foreach (Thing spot in map.listerBuildings.allBuildingsColonist)
                {
                    if (GuardPathDefOf.GetDefOfs().Contains(spot.def))
                    {
                        spot.DirtyMapMesh(map);
                    }
                }
            }

            //Widgets.Label(new Rect(LeftRectLabelUtility()), "GuardingP_OverrideAllowedAreaCheckBox".Translate());
            //Widgets.Checkbox(leftRectLabel.xMax + CheckboxPadding, rect.y, ref shouldOverrideAllowedArea);

            //Widgets.Checkbox((float)secondRectLabel.xMin - padding - Widgets.CheckboxSize, rect.y, ref shouldRenderAreaSpots);
            //Widgets.Label(new Rect(ThirdRectLabelUtility()), "GuardingP_GuardingAreaCheckBox".Translate());

        }


        /*private Rect LeftRectLabelUtility()
        {
            Rect LeftRectLabel = new(windowTabRect.xMin, windowTabRect.y, OverrideAllowedAreaWidth + 0.5f, windowTabRect.height);
            leftRectLabel = LeftRectLabel;
            return LeftRectLabel;
        }*/

        private Rect FirstRectLabelUtility()
        {
            Rect FirstRectLabel = new(windowTabRect.xMax - GuardingSpotKeyWidth - Widgets.CheckboxSize - CheckboxPadding * 2, windowTabRect.y, GuardingSpotKeyWidth, windowTabRect.height);
            firstRectLabel = FirstRectLabel;
            return FirstRectLabel;
        }

        private Rect SecondRectLabelUtility()
        {
            Rect SecondRectLabel = new(firstRectLabel.xMin - PatrollingSpotKeyWidth - Widgets.CheckboxSize - padding - CheckboxPadding * 2, windowTabRect.y, PatrollingSpotKeyWidth, windowTabRect.height);
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

        
    }
}
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

        private static readonly float GuardingSpotKeyWidth = Text.CalcSize("GuardingP_GuardingSpotCheckBox").x;
        private static readonly float PatrollingSpotKeyWidth = Text.CalcSize("GuardingP_PatrollingSpotCheckBox").x;
        private static readonly float GuardingAreaKeyWidth = Text.CalcSize("GuardingP_GuardingAreaCheckBox").x;

        bool True = true;

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.GuardingP_PawnTableDef_Guard;
        protected override IEnumerable<Pawn> Pawns => base.Pawns.Where((Pawn pawn) => pawn.IsFreeNonSlaveColonist && pawn.workSettings.WorkIsActive(WorkTypeDefOf.GuardingP_GuardingWorkType));
        

        public override void DoWindowContents(Rect rect)
        {
            windowTabRect = rect; 
            base.DoWindowContents(rect);

            Widgets.Checkbox((float)rect.xMax - Widgets.CheckboxSize - CheckboxPadding, rect.y, ref True);
            Widgets.Label(new Rect(FirstRectLabelUtility()), "GuardingP_GuardingSpotCheckBox".Translate());

            Widgets.Checkbox((float)firstRectLabel.xMin - padding - Widgets.CheckboxSize, rect.y, ref True);
            Widgets.Label(new Rect(SecondRectLabelUtility()), "GuardingP_PatrollingSpotCheckBox".Translate());

            Widgets.Checkbox((float)secondRectLabel.xMin - padding - Widgets.CheckboxSize, rect.y, ref True);
            Widgets.Label(new Rect(ThirdRectLabelUtility()), "GuardingP_GuardingAreaCheckBox".Translate());

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

        private Rect ThirdRectLabelUtility()
        {
            Rect ThirdRectLabel = new(secondRectLabel.xMin - GuardingAreaKeyWidth + Widgets.CheckboxSize + padding, windowTabRect.y, GuardingAreaKeyWidth, windowTabRect.height);
            return ThirdRectLabel;
        }


        protected override float ExtraTopSpace
        {
            get
            {
                return 30f;
            }
        }
    }
}

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Manipulation : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect rect2 = new(rect.x, rect.y, rect.width, rect.height);
            if (pawn.IsFreeNonSlaveColonist)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, (string)pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation).ToString("P0", System.Globalization.CultureInfo.InvariantCulture));


                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                {
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    TipSignal tooltip = pawn.GetTooltip();
                    tooltip.text = TranslatorFormattedStringExtensions.Translate("GuardingP_ManipulationColumn", pawn);
                    TooltipHandler.TipRegion(rect2, tooltip);
                }
            }
        }
    }
}

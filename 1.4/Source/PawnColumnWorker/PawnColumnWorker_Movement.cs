namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Movement : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect rect2 = new(rect.x, rect.y, rect.width, rect.height);
            if (pawn.IsFreeNonSlaveColonist)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, (string)pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving).ToString("P0", System.Globalization.CultureInfo.InvariantCulture));


                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                {
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    TipSignal tooltip = pawn.GetTooltip();
                    tooltip.text = TranslatorFormattedStringExtensions.Translate("GuardingP_MovementColumn", pawn);
                    TooltipHandler.TipRegion(rect2, tooltip);
                }
            }
        }
    }
}

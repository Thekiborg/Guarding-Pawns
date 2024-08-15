namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_ShootingLevel : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect rect2 = new(rect.x, rect.y, rect.width, rect.height);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, pawn.skills?.GetSkill(SkillDefOf.Shooting).GetLevel(true).ToString());

            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
                TipSignal tooltip = pawn.GetTooltip();
                tooltip.text = TranslatorFormattedStringExtensions.Translate("GuardingP_ShootingColumn", pawn);
                TooltipHandler.TipRegion(rect2, tooltip);
            }

        }
    }
}

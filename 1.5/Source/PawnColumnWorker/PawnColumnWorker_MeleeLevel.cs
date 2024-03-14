namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_MeleeLevel : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect rect2 = new(rect.x, rect.y, rect.width, rect.height);
            if (pawn.IsFreeNonSlaveColonist)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, (string)pawn.skills.GetSkill(SkillDefOf.Melee).GetLevel(true).ToString());

                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                {
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    TipSignal tooltip = pawn.GetTooltip();
                    tooltip.text = "GuardingP_MeleeColumn".Translate(pawn.NameShortColored);
                    TooltipHandler.TipRegion(rect2, tooltip);
                }
            }
        }
    }
}

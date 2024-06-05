namespace Thek_GuardingPawns
{
    /// <summary>
    /// Skips the aiAvoidCover check for mechanoids doing my jobs, so I don't make all the pawnkinds do it for all instances.
    /// </summary>
    [HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
    internal static class CastPositionFinder_CastPositionPreference_Transpiler
    {
        private static bool IsMechanoidAndGuarding()
        {
            return CastPositionFinder.req.caster.RaceProps.IsMechanoid &&(CastPositionFinder.req.caster.CurJobDef == GuardingJobsDefOf.GuardingP_GuardPawn
                || CastPositionFinder.req.caster.CurJobDef == GuardingJobsDefOf.GuardingP_GuardPath
                || CastPositionFinder.req.caster.CurJobDef == GuardingJobsDefOf.GuardingP_GuardSpot);
        }


        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SkipAiAvoidCoverCheck(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher codeMatcher = new(codeInstructions);


            // The set of instructions that it needs to find to know where to insert.
            var instructionsToMatch = new CodeMatch[]
            {
                /*
                 * This set of instructions correspond to this line:
                 * if (req.caster.kindDef.aiAvoidCover)
                 */
                Code.Ldsflda[AccessTools.Field(typeof(CastPositionFinder), "req")],
                Code.Ldfld[AccessTools.Field(typeof(CastPositionRequest), "caster")],
                Code.Ldfld[AccessTools.Field(typeof(Pawn), "kindDef")],
                Code.Ldfld[AccessTools.Field(typeof(PawnKindDef), "aiAvoidCover")],
                Code.Brfalse_S
            };


            // The instructions i want to insert
            var instructionsToInsert = new CodeInstruction[]
            {
                Code.Call[AccessTools.Method(typeof(CastPositionFinder_CastPositionPreference_Transpiler), nameof(IsMechanoidAndGuarding))],
                Code.Brtrue_S
            };

            codeMatcher.MatchEndForward(instructionsToMatch);

            if (codeMatcher.IsInvalid)
            {
                Log.Error("CastPositionFinder_CastPositionPreference_Transpiler couldn't patch it's intended method, find how to report it on Guarding Pawn's steam page.");
                return codeInstructions;
            }
            else
            {
                instructionsToInsert[1].operand = codeMatcher.Instruction.operand;

                codeMatcher.MatchStartBackwards(Code.Ldsflda[AccessTools.Field(typeof(CastPositionFinder), "req")]);

                codeMatcher.Insert(instructionsToInsert);
                return codeMatcher.InstructionEnumeration();
            }
        }
    }
}

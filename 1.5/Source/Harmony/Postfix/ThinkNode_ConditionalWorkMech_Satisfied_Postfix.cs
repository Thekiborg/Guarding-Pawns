namespace Thek_GuardingPawns
{
	/// <summary>
	/// Fixes incompatibility with Veltaris' Mechanoid Spots
	/// </summary>
	[HarmonyPatch(typeof(ThinkNode_ConditionalWorkMech), "Satisfied")]
	internal static class ThinkNode_ConditionalWorkMech_Satisfied_Postfix
	{
		[HarmonyPostfix]
		private static void GuardingWorkTypeIsNotWork(ref bool __result, Pawn pawn)
		{
			if (pawn.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDefOf.GuardingP_GuardingWorkType))
			{
				__result = false;
			}
		}
	}
}

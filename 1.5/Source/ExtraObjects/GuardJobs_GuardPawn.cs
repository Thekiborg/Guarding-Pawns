﻿namespace Thek_GuardingPawns
{
    internal class GuardJobs_GuardPawn : GuardJobs, IExposable
    {
        internal GuardJobs_GuardPawn()
        {
            pawnToGuard = null;
        }
        internal Pawn pawnToGuard;

        public override void ExposeData()
        {
            Scribe_References.Look(ref pawnToGuard, "pawnToGuard");
        }
    }
}
global using System;
global using System.Collections.Generic;
global using System.Reflection;
global using RimWorld;
global using Verse;
global using UnityEngine;
global using HarmonyLib;
global using Verse.AI;

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public static class GuardingPawns
    {

        internal static bool isVFEInsectoids2Active = ModsConfig.IsActive("oskarpotocki.vfe.insectoid2");

        static GuardingPawns()
        {
            Log.Message("<color=#702963>Thek was here:</color> Guarding Pawns <color=#702963>loaded!</color>");

            // Harmony init
            Harmony harmony = new("Thekiborg.GuardingPawns");
            harmony.PatchAll();
        }
    }
}

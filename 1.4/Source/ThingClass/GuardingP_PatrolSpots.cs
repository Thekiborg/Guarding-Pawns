namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class GuardingP_PatrolSpots : Building
    {
        private List<Thing> ListForDef;
        private static Dictionary<ThingDef, int> SpotCounter = new();
        private string resolvedLabel;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            Command_Action command_Action = new()
            {
                defaultLabel = "Change order",
                action = delegate ()
                {
                    Find.WindowStack.Add(new Window_SortPatrolSpots(Map, ListForDef));
                }
            };
            yield return command_Action;
        }

        public override string Label => resolvedLabel;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ChangeLabel();
            StoreThing();
        }

        private void ChangeLabel()
        {
            SpotCounter.TryAdd(def, 0);
            SpotCounter[def] += 1;
            resolvedLabel = $"{def.label} {SpotCounter[def]}";
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            UnstoreThing();
            base.DeSpawn(mode);
        }


        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            UnstoreThing();
            base.Destroy(mode);
        }


        private void StoreThing()
        {
            FindListForDef();
            ListForDef.Add(this);
        }


        private void UnstoreThing()
        {
            FindListForDef();
            ListForDef.Remove(this);
        }


        private void FindListForDef()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();
            if (def == GuardPathDefOf.GuardingP_redPatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().RedPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_yellowPatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().YellowPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_orangePatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().OrangePatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_greenPatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().GreenPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_bluePatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().BluePatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_purplePatrol)
            {
                ListForDef = MapHeld.GetComponent<MapComponent_GuardingPawns>().PurplePatrolsOnMap;
                return;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref resolvedLabel, "PatrolSpotLabel");
        }
    }
}

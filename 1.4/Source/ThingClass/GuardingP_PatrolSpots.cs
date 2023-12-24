namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class GuardingP_PatrolSpots : Building
    {
        private List<Thing> ListForDef;
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
                    Find.WindowStack.Add(new Window_SortPatrolSpots(Map));
                }
            };
            yield return command_Action;
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            StoreThing();
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
    }
}

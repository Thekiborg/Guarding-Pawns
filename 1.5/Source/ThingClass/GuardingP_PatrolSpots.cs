using System.Linq;

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class GuardingP_PatrolSpots : Building, ILoadReferenceable
    {
        private SortedList<int, Thing> ListForDef;
        private static readonly Dictionary<ThingDef, int> SpotCounter = new();
        private string resolvedLabel;
        private int order;


        public override void Print(SectionLayer layer)
        {
            if (MainTabWindow_Guards.shouldRenderPatrollingSpots)
            {
                base.Print(layer);
            }
        }


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
            FindListForDef();
            StoreThing();
        }


        private void ChangeLabel()
        {
            if (resolvedLabel != null)
            {
                return;
            }
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
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();

            FindListForDef();
            while (ListForDef.ContainsKey(order))
            {
                order += 1;
            }
            ListForDef.TryAdd(order, this);
            mapComp.PatrolSpotsOnMap.Add(this);
        }


        private void UnstoreThing()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();

            FindListForDef();
            int indexForDeletion = ListForDef.IndexOfValue(this);
            ListForDef.Remove(indexForDeletion);
            if (mapComp.PatrolSpotsOnMap.Contains(this))
            {
                mapComp.PatrolSpotsOnMap.Remove(this);
            }
        }


        private void FindListForDef()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();
            if (def == GuardPathDefOf.GuardingP_redPatrol)
            {
                ListForDef = mapComp.RedPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_yellowPatrol)
            {
                ListForDef = mapComp.YellowPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_orangePatrol)
            {
                ListForDef = mapComp.OrangePatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_greenPatrol)
            {
                ListForDef = mapComp.GreenPatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_bluePatrol)
            {
                ListForDef = mapComp.BluePatrolsOnMap;
                return;
            }
            if (def == GuardPathDefOf.GuardingP_purplePatrol)
            {
                ListForDef = mapComp.PurplePatrolsOnMap;
                return;
            }
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                order = ListForDef.IndexOfValue(this);
            }
            base.ExposeData();
            Scribe_Values.Look(ref order, "GuardingP_OrderInList");
            Scribe_Values.Look(ref resolvedLabel, "GuardingP_PatrolSpotLabel");
        }
    }
}

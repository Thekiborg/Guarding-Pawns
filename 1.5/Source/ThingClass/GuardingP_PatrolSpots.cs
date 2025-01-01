using System.Linq;

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class GuardingP_PatrolSpots : Building, ILoadReferenceable
    {
        private SortedList<int, Thing> ListForDef;
        private int spawnOrder;
        private readonly CachedTexture gizmoIcon = new("Gizmo/SortPatrolSpotsIcon");

        internal string newLabel;

		public override string Label => newLabel ?? def.label;


		public override void Print(SectionLayer layer)
        {
            if (MapComponent_GuardingPawns.shouldRenderPatrollingSpots)
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

            yield return new Command_Action()
            {
                defaultLabel = "GuardingP_ChangeOrderGizmo".Translate(),
                icon = gizmoIcon.Texture,
                action = delegate ()
                {
                    Find.WindowStack.Add(new Window_SortPatrolSpots(ListForDef));
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Rename".Translate(),
                icon = TexButton.Rename,
                action = delegate ()
                {
                    Find.WindowStack.Add(new Window_RenamePatrolSpots(this));
                },
            };
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


        private void StoreThing()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();

            FindListForDef();
            while (ListForDef.ContainsKey(spawnOrder))
            {
                spawnOrder += 1;
            }
            ListForDef.TryAdd(spawnOrder, this);
            mapComp.PatrolSpotsOnMap.Add(this);
        }


        private void UnstoreThing()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();

            FindListForDef();
            int indexForDeletion = ListForDef.IndexOfValue(this);
            ListForDef.RemoveAt(indexForDeletion);
            mapComp.PatrolSpotsOnMap.Remove(this);
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
                spawnOrder = ListForDef.IndexOfValue(this);
            }
            base.ExposeData();
            Scribe_Values.Look(ref spawnOrder, "GuardingP_OrderInList");
            Scribe_Values.Look(ref newLabel, "GuardingP_PatrolSpotLabel");
        }
    }
}

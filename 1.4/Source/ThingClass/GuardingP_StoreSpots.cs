namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class GuardingP_StoreSpots : Building
    {
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
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();
            mapComp.spotsOnMap.Add(this);
        }


        private void UnstoreThing()
        {
            MapComponent_GuardingPawns mapComp = MapHeld.GetComponent<MapComponent_GuardingPawns>();
            if (mapComp.spotsOnMap.Contains(this))
            {
                MapHeld.GetComponent<MapComponent_GuardingPawns>().spotsOnMap.Remove(this);
            }
        }
    }
}

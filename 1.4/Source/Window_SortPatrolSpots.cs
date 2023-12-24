using UnityEngine.UI;
using Verse;

namespace Thek_GuardingPawns
{
    public class Window_SortPatrolSpots : Window
    {
        public Map Map;
        public Window_SortPatrolSpots(Map map)
        {
            Map = map;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect2 = inRect;
            rect2.yMax -= 120f;
            Widgets.DrawMenuSection(rect2);
            DoListEntries(rect2);
            //Rect rect3 = new(inRect.width, rect2.yMax - 10f, rect2.width, inRect.height - rect2.height);
            //Widgets.TextArea(rect3, "Heloafdasf", true);
        }

        private void DoListEntries(Rect rect)
        {
            Listing_Standard listing_Standard = new();
            listing_Standard.Begin(rect);
            foreach (Thing thing in Map.GetComponent<MapComponent_GuardingPawns>().RedPatrolsOnMap)
            {
                listing_Standard.Label(thing.Label);
            }
            listing_Standard.End();
        }

        public override Vector2 InitialSize => new(450, 750);
    }
}

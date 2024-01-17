﻿using Verse.Sound;

namespace Thek_GuardingPawns
{
    public class Window_SortPatrolSpots : Window
    {
        //debe hacer cambios directamente en la sortedlist
        const float UpButtonWidth = 24f;
        const float UpButtonHeight = 24f;
        const float padding = 12f;
        public Map Map;
        public SortedList<int, Thing> ListForDef;
        private static ScrollViewStatus _scrollViewStatus = new();
        public Window_SortPatrolSpots(Map map, SortedList<int, Thing> listForDef)
        {
            Map = map;
            ListForDef = listForDef;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect2 = inRect;
            rect2.yMax -= 120f;
            Widgets.DrawMenuSection(rect2);
            DoListEntries(rect2);

            Rect textRect = new(inRect.x, rect2.yMax + 10f, inRect.width, inRect.height - rect2.height - 50f);
            Widgets.Label(textRect, "SpotGUIexplanation".Translate());
            Rect closeRect = new(inRect.center.x - textRect.width / 8, inRect.yMax - 30f, textRect.width / 4, 30f);
            if (Widgets.ButtonText(closeRect, "SpotGUIbutton".Translate())) Close();
        }

        private void DoListEntries(Rect Rect)
        {
            Rect listingRect = new(Rect.x + 5f, Rect.y + 5f, Rect.width - 10f, Rect.height - 20f);
            using var scrollView = new ScrollView(listingRect, _scrollViewStatus);

            Listing_Standard listing_Standard = new();
            listing_Standard.Begin(scrollView.Rect with { height = float.PositiveInfinity });
            //The arguments are given by Bradson i don't know what the fuck this "with { height ...}" even does

            for (int i = 0; i != ListForDef.Count; i++)
            {
                Rect UpButtonRect = new(scrollView.Rect.xMax - (UpButtonWidth + 5f + padding) * 2, listing_Standard.CurHeight + 3f, UpButtonWidth, UpButtonHeight);
                Rect DownButtonRect = new(UpButtonRect.xMax + 1f + padding, listing_Standard.CurHeight + 3f, UpButtonRect.width, UpButtonRect.height);
                Rect highlightRect = new(scrollView.Rect.xMin, listing_Standard.CurHeight, Rect.width, UpButtonHeight);

                Thing thing = ListForDef[i];
                listing_Standard.Label(thing.Label);

                //Move upwards (The top of the list is index 0)
                if (i != 0 && Widgets.ButtonImage(UpButtonRect, TexButton.ReorderUp))
                {
                    soundClose.PlayOneShotOnCamera();
                    ListForDef.TryGetValue(i - 1, out Thing thingToRelocate); //Value of the place we want to be at
                    ListForDef.TryGetValue(i, out Thing thingToBeMoved); //Current value
                    ListForDef[i - 1] = thingToBeMoved;
                    ListForDef[i] = thingToRelocate;
                    //If we want to move T2 to T1's position
                    //T1's index is i - 1, T2's index is i
                    //T1 will be moved to i while T2 will be moved to i - 1
                }

                //Move downwards
                if (i != ListForDef.Count - 1 && Widgets.ButtonImage(DownButtonRect, TexButton.ReorderDown))
                {
                    soundClose.PlayOneShotOnCamera();
                    ListForDef.TryGetValue(i + 1, out Thing pastThing);
                    ListForDef.TryGetValue(i, out Thing futureThing);
                    ListForDef[i + 1] = futureThing;
                    ListForDef[i] = pastThing;

                }

                if (Mouse.IsOver(highlightRect))
                {
                    GUI.DrawTexture(highlightRect, TexUI.HighlightTex);
                }

            }
            _scrollViewStatus.Height = listing_Standard.CurHeight;
            listing_Standard.End();
        }

        public override Vector2 InitialSize => new(450, 750);
    }


    ///<summary>
    /// Code from Bradson's Adaptive Storage Framework, makes it easier to do scrollings.
    /// Goes with the ScrollViewStatus class.
    /// </summary>
    public readonly struct ScrollView : IDisposable
    {
        private readonly ScrollViewStatus _scrollViewStatus;

        public readonly Rect Rect;

        public ref float Height => ref _scrollViewStatus.Height;

        public ScrollView(Rect outRect, ScrollViewStatus scrollViewStatus, bool showScrollbars = true)
        {
            _scrollViewStatus = scrollViewStatus;
            // var viewRect = outRect with { width = outRect.width - 20f, height = _scrollViewStatus.Height };
            Rect = new(0f, 0f, outRect.width, Math.Max(_scrollViewStatus.Height, outRect.height));
            if (_scrollViewStatus.Height - 0.1f >= outRect.height)
                Rect.width -= 16f;

            scrollViewStatus.Height = 0f;
            Widgets.BeginScrollView(outRect, ref _scrollViewStatus.Position, Rect, showScrollbars);
        }

        public void Dispose() => Widgets.EndScrollView();
    }

    ///<summary>
    /// Code from Bradson's Adaptive Storage Framework, makes it easier to do scrollings.
    /// Goes with the ScrollView struct.
    /// </summary>
    public class ScrollViewStatus
    {
        public Vector2 Position;
        public float Height;
    }
}

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_SelectJobExtras : PawnColumnWorker
    {
        private Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new(); //Caching
        private MapComponent_GuardingPawns guardAssignmentsMapComp;


        public enum GuardSpotGroupColor
        {
            GuardingP_redSpot,
            GuardingP_yellowSpot,
            GuardingP_orangeSpot,
            GuardingP_greenSpot,
            GuardingP_blueSpot,
            GuardingP_purpleSpot,
        }
        public enum GuardPathGroupColor
        {
            GuardingP_redPath,
            GuardingP_yellowPath,
            GuardingP_orangePath,
            GuardingP_greenPath,
            GuardingP_bluePath,
            GuardingP_purplePath,
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!MapCompCache.ContainsKey(pawn.MapHeld)) //Checks if the dictionary has the MapComponent.
            {
                MapCompCache.Add(pawn.MapHeld, pawn.Map.GetComponent<MapComponent_GuardingPawns>());
                //If it isn't, add the MapComponent as value, with the current map as a key.
            }

            guardAssignmentsMapComp = MapCompCache.TryGetValue(pawn.MapHeld);
            
            var pawnJobType = guardAssignmentsMapComp.GuardJobs.TryGetValue(pawn);


            if (pawn.IsFreeNonSlaveColonist) //Slaves don't guard, silly.
            {
                Listing_Standard listing_StandardGuardAssignments = new();
                switch (pawnJobType)
                {
                    case GuardJobs_GuardSpot spot:

                        listing_StandardGuardAssignments.Begin(rect);

                        if (listing_StandardGuardAssignments.ButtonText(
                            label: spot.SpotColor.ToString().Translate()
                            ))
                        {
                            GuardSpotExtraOptions(pawn, rect);
                        }

                        listing_StandardGuardAssignments.End();
                        break;


                    case GuardJobs_GuardPath path:
                        float checkboxPadding = 7.5f;

                        Rect rectButton = new(rect.xMin, rect.yMin, rect.width / 2, rect.height);

                        listing_StandardGuardAssignments.Begin(rectButton);

                        if (listing_StandardGuardAssignments.ButtonText(
                            label: path.PathColor.ToString().Translate()
                            ))
                        {
                            GuardPathExtraOptions(pawn, rectButton);
                        }
                        listing_StandardGuardAssignments.End();

                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label(new Rect(rectButton.xMax, rect.yMin, rect.width - rectButton.width - Widgets.CheckboxSize - checkboxPadding, rect.height), "ShouldLoop".Translate());
                        Text.Anchor = TextAnchor.UpperLeft;

                        Widgets.Checkbox(rect.xMax - Widgets.CheckboxSize - checkboxPadding, rect.y, ref path.shouldLoop);
                        
                        break;


                    case GuardJobs_GuardPawn pn:

                        listing_StandardGuardAssignments.Begin(rect);

#pragma warning disable CS0618 // Type or member is obsolete
                        if (listing_StandardGuardAssignments.ButtonText(
                            label: "GuardingP_ProtectPawn".ToString().Translate(pn.pawnToGuard.Name)
                            ))
                        {
                            GuardPawnExtraOptions(pawn, rect);
                        }
#pragma warning restore CS0618 // Type or member is obsolete

                        listing_StandardGuardAssignments.End();
                        break;


                    case null:
                    default:

                        //listing_StandardGuardAssignments.Begin(rect);

                        //listing_StandardGuardAssignments.End();
                        break;
                }
            }
        }


        private void GuardSpotExtraOptions(Pawn pawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();
            //All the menu buttons go in here

            foreach (GuardSpotGroupColor colorGroup in Enum.GetValues(typeof(GuardSpotGroupColor))) //Iterates through the enum
            {
                menuOptions.Add(new(colorGroup.ToString().Translate(), () => //Makes a new button for each one of the cons inside the enum
                {
                    //This all runs once the menu button is clicked
                    GuardJobs_GuardSpot guardJobs_GuardSpot = new()
                    {
                        pawn = pawn,
                        SpotColor = colorGroup,
                    };

                    guardAssignmentsMapComp.GuardJobs[pawn] = guardJobs_GuardSpot;
                    //Changes the colorGroup value in the dictionary for our current pawn.
                }));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions)); //Creates the buttons from the list

            if (Mouse.IsOver(rect)) //Hi gh li gh t!
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }


        private void GuardPathExtraOptions(Pawn pawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();

            foreach (GuardPathGroupColor colorGroup in Enum.GetValues(typeof(GuardPathGroupColor)))
            {
                menuOptions.Add(new(colorGroup.ToString().Translate(), () =>
                {
                    GuardJobs_GuardPath guardJobs_GuardPath = new()
                    {
                        pawn = pawn,
                        PathColor = colorGroup,
                    };

                    guardAssignmentsMapComp.GuardJobs[pawn] = guardJobs_GuardPath;
                }));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));

            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }


        private void GuardPawnExtraOptions(Pawn windowTabPawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();

            foreach (Pawn pawnToProtect in windowTabPawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (pawnToProtect != windowTabPawn)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    menuOptions.Add(new("GuardingP_ProtectPawn".ToString().Translate(pawnToProtect.Name), () =>
                    {
                        GuardJobs_GuardPawn guardJobs_GuardPawn = new()
                        {
                            pawn = windowTabPawn,
                            pawnToGuard = pawnToProtect,
                        };

                        guardAssignmentsMapComp.GuardJobs[windowTabPawn] = guardJobs_GuardPawn;
                    }));
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));

            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }
    }
}
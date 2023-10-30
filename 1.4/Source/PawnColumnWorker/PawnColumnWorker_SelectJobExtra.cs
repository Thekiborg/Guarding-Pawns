namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_SelectJobExtras : PawnColumnWorker
    {
        public Dictionary<Map, MapComponent_WindowTabButtonSelection> MapCompInCurrentMap = new(); //Caching
        private MapComponent_WindowTabButtonSelection dictClass;

        public static Dictionary<Pawn, PawnColumnWorker_SelectJobExtras.GuardPathGroupColor> PathColor = new();
        public enum GuardSpotGroupColor
        {
            GuardingP_redSpot,
            GuardingP_orangeSpot,
            GuardingP_yellowSpot,
            GuardingP_blueSpot,
            GuardingP_purpleSpot,
        }
        public enum GuardPathGroupColor
        {
            GuardingP_redPath,
            GuardingP_orangePath,
            GuardingP_yellowPath,
            GuardingP_bluePath,
            GuardingP_purplePath,
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!MapCompInCurrentMap.ContainsKey(pawn.MapHeld)) //Checks if the dictionary has the MapComponent.
            {
                MapCompInCurrentMap.Add(pawn.MapHeld, pawn.Map.GetComponent<MapComponent_WindowTabButtonSelection>());
                //If it isn't, add the MapComponent as value, with the current map as a key.
            }

            dictClass = MapCompInCurrentMap.TryGetValue(pawn.MapHeld);
            //We use the caching here, to get the MapComponent we want, since GetComponent() is slow.
            if (!dictClass.GuardSpotColorButtonSelection.ContainsKey(pawn))
            {
                dictClass.GuardSpotColorButtonSelection.TryAdd(pawn, GuardSpotGroupColor.GuardingP_redSpot);
                //Now if the GuardSpotColorButtonSelection dictionary doesn't contain this pawn, it gets added, with the redSpot value as the default.
            }
            GuardSpotGroupColor buttonGuardingSpotLabel = dictClass.GuardSpotColorButtonSelection.TryGetValue(pawn);
            //This one grabs the color group value assigned to the pawn in the dictionary, to use it for the button's label.


            if (!dictClass.GuardPathColorButtonSelection.ContainsKey(pawn))
            {
                dictClass.GuardPathColorButtonSelection.TryAdd(pawn, GuardPathGroupColor.GuardingP_redPath);
            }
            GuardPathGroupColor buttonGuardingPathLabel = dictClass.GuardPathColorButtonSelection.TryGetValue(pawn);


            if (!dictClass.GuardPawnButtonSelection.ContainsKey(pawn))
            {
                dictClass.GuardPawnButtonSelection.TryAdd(pawn, pawn);
            }
            Pawn buttonGuardingPawnLabel = dictClass.GuardPawnButtonSelection.TryGetValue(pawn);


            PawnColumnWorker_SelectJob.GuardJobType pawnJobType = dictClass.JobButtonSelection.TryGetValue(pawn);
            //Grabs the guarding job assigned to the pawn

            if (pawn.IsFreeNonSlaveColonist) //Slaves don't guard, silly.
            {
                //Now we do different buttons for each one of the job types.
                switch (pawnJobType)
                {
                    case PawnColumnWorker_SelectJob.GuardJobType.GuardingP_GuardSpot:

                        Listing_Standard listing_StandardGuardSpot = new();
                        listing_StandardGuardSpot.Begin(rect);

                        if (listing_StandardGuardSpot.ButtonText(
                            label: buttonGuardingSpotLabel.ToString().Translate()
                            ))
                        {
                            GuardSpotExtraOptions(pawn, rect);
                        }

                        listing_StandardGuardSpot.End();
                        break;

                    case PawnColumnWorker_SelectJob.GuardJobType.GuardingP_GuardPath:

                        Listing_Standard listing_StandardGuardPath = new();
                        listing_StandardGuardPath.Begin(rect);

                        if (listing_StandardGuardPath.ButtonText(
                            label: buttonGuardingPathLabel.ToString().Translate()
                            ))
                        {
                            GuardPathExtraOptions(pawn, rect);
                        }
                        listing_StandardGuardPath.End();
                        break;

                    case PawnColumnWorker_SelectJob.GuardJobType.GuardingP_GuardPawn: //Este no es null-safe, hay que prevenir más tarde

                        Listing_Standard listing_StandardGuardPawn = new();
                        listing_StandardGuardPawn.Begin(rect);

                        if (listing_StandardGuardPawn.ButtonText(
                            label: buttonGuardingPawnLabel.Name.ToString()
                            ))
                        {
                            GuardPawnExtraOptions(pawn, rect);
                        }

                        listing_StandardGuardPawn.End();
                        break;
                }
            }
        }

        /// <summary>
        /// Allows to select between the different colors for the guarding spot job, and changes it both on the GuardSpotColorButtonSelection dictionary, as well as in the button's label.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="rect"></param>
        private void GuardSpotExtraOptions(Pawn pawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();
            //All the menu buttons go in here

            foreach (GuardSpotGroupColor colorGroup in Enum.GetValues(typeof(GuardSpotGroupColor))) //Iterates through the enum
            {
                menuOptions.Add(new(colorGroup.ToString().Translate(), () => //Makes a new button for each one of the cons inside the enum
                {
                    //This all runs once the menu button is clicked
                    if (dictClass.GuardSpotColorButtonSelection.ContainsKey(pawn))//I'm verifying the pawn is inside the dictionary
                    {
                        dictClass.GuardSpotColorButtonSelection[pawn] = colorGroup;
                        //Changes the colorGroup value in the dictionary for our current pawn.
                    }
                    else //If it isn't, i simply add it with the default value
                    {
                        dictClass.GuardSpotColorButtonSelection.TryAdd(pawn, GuardSpotGroupColor.GuardingP_redSpot);
                    }
                }));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions)); //Creates the buttons from the list

            if (Mouse.IsOver(rect)) //Hi gh li gh t!
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }

        /// <summary>
        /// Allows to select between the different colors for the patrol paths
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="rect"></param>
        private void GuardPathExtraOptions(Pawn pawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();

            foreach (GuardPathGroupColor colorGroup in Enum.GetValues(typeof(GuardPathGroupColor)))
            {
                menuOptions.Add(new(colorGroup.ToString(), () =>
                {
                    if (dictClass.GuardPathColorButtonSelection.ContainsKey(pawn))
                    {
                        dictClass.GuardPathColorButtonSelection[pawn] = colorGroup;
                        if (!PathColor.ContainsKey(pawn))
                        {
                            PathColor.TryAdd(pawn, colorGroup);
                        }
                    }
                    else
                    {
                        dictClass.GuardPathColorButtonSelection.TryAdd(pawn, GuardPathGroupColor.GuardingP_redPath);
                    }
                }));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));

            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }

        /// <summary>
        /// Allows to select between the different
        /// </summary>
        /// <param name="windowTabPawn"></param>
        /// <param name="rect"></param>
        /// <param name="freeColonistSpawnedPawns"></param>
        private void GuardPawnExtraOptions(Pawn windowTabPawn, Rect rect)
        {
            List<FloatMenuOption> menuOptions = new();

            foreach (Pawn pawnToProtect in windowTabPawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (pawnToProtect.teleporting)
                {
                    continue;
                }
#pragma warning disable CS0618 // Type or member is obsolete
                menuOptions.Add(new("GuardingP_ProtectPawn".Translate(pawnToProtect.Name), () =>
                {
                    if (dictClass.GuardPawnButtonSelection.ContainsKey(windowTabPawn))
                    {
                        dictClass.GuardPawnButtonSelection[windowTabPawn] = pawnToProtect;
                    }
                    else
                    {
                        dictClass.GuardPawnButtonSelection.TryAdd(windowTabPawn, windowTabPawn);
                    }
                }));
#pragma warning restore CS0618 // Type or member is obsolete
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));

            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture (rect, TexUI.HighlightTex);
            }
        }
    }
}
using System.Linq;

namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_SelectJob : PawnColumnWorker
    {
        private MapComponent_WindowTabButtonSelection dictClass;
        public Dictionary<Map, MapComponent_WindowTabButtonSelection> MapCompInCurrentMap = new();
        public enum GuardJobType
        {
            GuardingP_Undefined,
            GuardingP_GuardSpot,
            GuardingP_GuardArea,
            GuardingP_GuardPawn,
            GuardingP_GuardPath
        }

        
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!MapCompInCurrentMap.ContainsKey(pawn.MapHeld))
            {
                MapCompInCurrentMap.Add(pawn.MapHeld, pawn.Map.GetComponent<MapComponent_WindowTabButtonSelection>());
            }
            
            dictClass = MapCompInCurrentMap.TryGetValue(pawn.MapHeld);
            //Gets this map's MapComponent_WindowTabButtonSelection. This gets the component from each map.

            if (!dictClass.JobButtonSelection.ContainsKey(pawn))
            {
                dictClass.JobButtonSelection.TryAdd(pawn, GuardJobType.GuardingP_Undefined);
                //Creates a dictionary entry, having the pawn as a key and the undefined job as it's value
            }

            Listing_Standard listing_Standard = new();
            //Rect rect2 = new(rect.x, rect.y, rect.width, rect.height);
            
            GuardJobType buttonLabel = dictClass.JobButtonSelection.TryGetValue(pawn);
            //Grabs the value tied to the pawn key, which is the job selected by the button.


            if (pawn.IsFreeNonSlaveColonist) //Only makes the list if the pawn is not a slave.
            {
                listing_Standard.Begin(rect); //This needs a listing_Standard.End(), else the gui crashes.

                if (listing_Standard.ButtonText( //Fires once the button is pressed
                    label: buttonLabel.ToString().Translate()
                    ))
                {
                    TryCleanupDict();
                    DoFloatMenuButtons(pawn);
                }
                listing_Standard.End(); //Ends the listing_Standard, we don't want the gui to crash... Right?
            }
        }

        private void DoFloatMenuButtons(Pawn pawn)
        {
            List<FloatMenuOption> menuOptions = new(); // List for adding the float menu options

            foreach (GuardJobType guardJob in Enum.GetValues(typeof(GuardJobType))) // Enum.GetValues(typeof(GuardJobType)) returns an array of the values in the enum
            // For each value inside the enum (array)
            {
                if (guardJob != GuardJobType.GuardingP_Undefined) // We exclude the undefined job type, as we don't want the player selecting this one, it's just a placeholder
                {
                    menuOptions.Add(new(guardJob.ToString().Translate(), () =>
                    // Creates a button per value inside the enum, except for the undefined one.
                    {
                        if (dictClass.JobButtonSelection.ContainsKey(pawn)) //If the dictionary contains that pawn registered as key
                        {
                            dictClass.JobButtonSelection[pawn] = guardJob;
                            //It will change the value (using the pawn key to know which value to change) of currentJob, and the label of the button, successfully changing the job we've selected
                        }
                        else
                        {
                            dictClass.JobButtonSelection.Add(pawn, GuardJobType.GuardingP_Undefined);
                            //If the pawn is not in the dictionary for whatever reason we create the entry here and give it the undefined job.
                        }


                        foreach (KeyValuePair<Pawn, GuardJobType> i in dictClass.JobButtonSelection) //Logging
                        {
                            Log.Error(i.Value + " " + i.Key);
                        }
                    }));
                }
            }
            Find.WindowStack.Add(new FloatMenu(menuOptions));
        }

        /// <summary>
        /// Utility Method for clearing dead or destroyed pawns, to not kill the save or something lol
        /// </summary>
        private void TryCleanupDict()
        {
            var keys = dictClass.JobButtonSelection.Keys;
            //The key value for all the entries in the dictionary.
            List<Pawn> pawnListForDeleting = keys.Where(pawn => pawn.Dead || pawn.Destroyed || pawn == null).ToList();
            //Makes a list and adds all the keys whose pawn is dead or destroyed or null

            foreach (Pawn pawn in pawnListForDeleting)
            //Iterates through each pawn that was deleted, but is still in the dictionary saved.
            {
                dictClass.JobButtonSelection.Remove(pawn);
                //Removes them from the dictionary.
            }
        }
    }
}

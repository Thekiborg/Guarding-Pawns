﻿namespace Thek_GuardingPawns
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_SelectJob : PawnColumnWorker
    {
        private MapComponent_GuardingPawns guardAssignmentsMapComp;
        public Dictionary<Map, MapComponent_GuardingPawns> MapCompCache = new();
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
            if (pawn != null && pawn.Spawned)
            {
                if (!pawn.RaceProps.IsMechanoid || (pawn.RaceProps.IsMechanoid && !pawn.IsColonyMechRequiringMechanitor())) //Only makes the list if the pawn is not a slave.
                {
                    if (!MapCompCache.ContainsKey(pawn.MapHeld)) MapCompCache.TryAdd(pawn.MapHeld, pawn.MapHeld.GetComponent<MapComponent_GuardingPawns>());

                    guardAssignmentsMapComp = MapCompCache.TryGetValue(pawn.MapHeld);
                    //Gets this map's MapComponent_WindowTabButtonSelection. This gets the component from each map.

                    var buttonLabel = guardAssignmentsMapComp.GuardJobs.TryGetValue(pawn);
                    //Grabs the value tied to the pawn key, which is the job selected by the button.

                    Listing_Standard listing_Standard = new();
                    listing_Standard.Begin(rect); //This needs a listing_Standard.End(), else the gui crashes.

                    var label = buttonLabel?.ToString() ?? GuardJobType.GuardingP_Undefined.ToString();
                    if (listing_Standard.ButtonText(label.Translate()))
                    {
                        DoFloatMenuButtons(pawn);
                    }
                    listing_Standard.End(); //Ends the listing_Standard, we don't want the gui to crash... Right?
                }
            }
        }

        private void DoFloatMenuButtons(Pawn pawn)
        {
            List<FloatMenuOption> menuOptions = new(); // List for adding the float menu options

            foreach (GuardJobType guardJob in Enum.GetValues(typeof(GuardJobType))) // Enum.GetValues(typeof(GuardJobType)) returns an array of the values in the enum
            // For each value inside the enum (array)
            {
                switch (guardJob)
                {
                    case GuardJobType.GuardingP_Undefined:
                        menuOptions.Add(new(guardJob.ToString().Translate(), () =>
                        {
                            guardAssignmentsMapComp.GuardJobs[pawn] = null;
                        }));
                        break;


                    case GuardJobType.GuardingP_GuardSpot:

                        menuOptions.Add(new(guardJob.ToString().Translate(), () =>
                        {
                            GuardJobs_GuardSpot guardJobs_GuardSpot = new()
                            {
                                pawn = pawn,
                                SpotColor = PawnColumnWorker_SelectJobExtras.GuardSpotGroupColor.GuardingP_redSpot
                            };

                            guardAssignmentsMapComp.GuardJobs[pawn] = guardJobs_GuardSpot;
                        }));
                        break;


                    case GuardJobType.GuardingP_GuardPath:

                        menuOptions.Add(new(guardJob.ToString().Translate(), () =>
                        {
                            GuardJobs_GuardPath guardJobs_GuardPath = new()
                            {
                                pawn = pawn,
                                PathColor = PawnColumnWorker_SelectJobExtras.GuardPathGroupColor.GuardingP_redPath
                            };

                            guardAssignmentsMapComp.GuardJobs[pawn] = guardJobs_GuardPath;
                        }));
                        break;


                    case GuardJobType.GuardingP_GuardPawn:

                        menuOptions.Add(new(guardJob.ToString().Translate(), () =>
                        {
                            GuardJobs_GuardPawn guardJobs_GuardPawn = new()
                            {
                                pawn = pawn,
                                pawnToGuard = pawn,
                            };

                            guardAssignmentsMapComp.GuardJobs[pawn] = guardJobs_GuardPawn;
                        }));
                        break;
                }

            }
            Find.WindowStack.Add(new FloatMenu(menuOptions));
        }
    }
}